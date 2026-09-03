import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of, switchMap } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { MovieService } from '../../../movie/services/movie.service';
import { CinemaService } from '../../../cinema/services/cinema.service';
import { SeatResponse } from '../../../cinema/models/cinema.model';
import { ScreeningService } from '../../../screening/services/screening.service';
import { Screening } from '../../../screening/models/screening.model';
import { AuthService } from '../../../auth/services/auth.service';
import { SEAT_PRICE } from '../../models/reservation.model';
import { ReservationService } from '../../services/reservation.service';

interface BookableSeat extends SeatResponse {
  available: boolean;
}

interface SeatRow {
  row: number;
  seats: BookableSeat[];
}

@Component({
  selector: 'app-reservation-form',
  standalone: false,
  templateUrl: './reservation-form.component.html',
  styleUrl: './reservation-form.component.css',
})
export class ReservationFormComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly reservationService = inject(ReservationService);
  private readonly screeningService = inject(ScreeningService);
  private readonly movieService = inject(MovieService);
  private readonly cinemaService = inject(CinemaService);
  private readonly auth = inject(AuthService);

  screenings: Screening[] = [];
  private allFutureScreenings: Screening[] = [];
  private movieTitles = new Map<string, string>();

  selectedScreeningId = '';
  screening: Screening | null = null;

  rows: SeatRow[] = [];
  private seatById = new Map<string, BookableSeat>();
  readonly selectedSeatIds = new Set<string>();

  loadingScreenings = true;
  loadingSeats = false;
  submitting = false;
  error: string | null = null;
  seatError: string | null = null;
  cardNumber: string | null | undefined = null;

  filterMovieTitle: string | null = null;


  ngOnInit(): void {
    this.cardNumber = this.auth.user()?.cardNumber;
    forkJoin({
      screenings: this.screeningService.getScreenings().pipe(catchError(() => of([] as Screening[]))),
      movies: this.movieService.getMovies(1, 100).pipe(catchError(() => of(null))),
    }).subscribe(({ screenings, movies }) => {
      this.movieTitles = new Map((movies?.data ?? []).map((m) => [m.id, m.title]));
      this.allFutureScreenings = [...screenings]
        .filter((s) => new Date(s.startTime).getTime() > Date.now())
        .sort((a, b) => a.startTime.localeCompare(b.startTime));
      this.screenings = this.allFutureScreenings;

      const params = this.route.snapshot.queryParamMap;

      // ?movieId= from a movie page -> only that movie's screenings.
      const movieId = params.get('movieId');
      if (movieId) {
        this.screenings = this.allFutureScreenings.filter((s) => s.movieId === movieId);
        this.filterMovieTitle = this.movieTitles.get(movieId) ?? null;
      }

      this.loadingScreenings = false;
      // 86 linija 
      // Preselect an explicit ?screeningId=, or the only option when there is one.
      const preselect = params.get('screeningId');
      if (preselect && this.screenings.some((s) => s.id === preselect)) {
        this.selectedScreeningId = preselect;
        this.onScreeningChange();
      } else if (this.screenings.length === 1) {
        this.selectedScreeningId = this.screenings[0].id;
        this.onScreeningChange();
      }
    });
  }

  showAllScreenings(): void {
    this.filterMovieTitle = null;
    this.screenings = this.allFutureScreenings;
    this.selectedScreeningId = '';
    this.onScreeningChange();
  }

  screeningLabel(screening: Screening): string {
    const title = this.movieTitles.get(screening.movieId) ?? '(unknown movie)';
    const when = new Date(screening.startTime).toLocaleString();
    return `${title} — ${when}`;
  }

  onScreeningChange(): void {
    this.rows = [];
    this.seatById.clear();
    this.selectedSeatIds.clear();
    this.screening = null;
    this.seatError = null;
    this.error = null;

    if (!this.selectedScreeningId) return;

    // Which screening this load is for. The user can pick another screening
    // before the requests below resolve; without this check a slow response for
    // the old screening could land last and paint the wrong hall's seat map.
    const requestedScreeningId = this.selectedScreeningId;

    this.loadingSeats = true;
    this.screeningService
      .getScreening(requestedScreeningId)
      .pipe(
        switchMap((screening) =>
          forkJoin({
            screening: of(screening),
            available: this.reservationService.getAvailableSeats(screening.id),
            seats: this.cinemaService.getSeatsByCinemaAndHallIds(screening.cinemaId, screening.hallId),
          }),
        ),
      )
      .subscribe({
        next: ({ screening, available, seats }) => {
          // A newer selection has already superseded this request - drop it.
          if (requestedScreeningId !== this.selectedScreeningId) return;

          this.screening = screening;
          const availableIds = new Set(available.availableSeats);
          const bookable: BookableSeat[] = seats.map((s) => ({
            ...s,
            available: availableIds.has(s.id),
          }));
          for (const seat of bookable) this.seatById.set(seat.id, seat);
          this.rows = this.groupByRow(bookable);
          this.loadingSeats = false;
        },
        error: (err: HttpErrorResponse) => {
          if (requestedScreeningId !== this.selectedScreeningId) return;
          this.loadingSeats = false;
          console.error('Could not load seats for screening', err);
          this.seatError = 'Could not load the seat map for this screening.';
        },
      });
  }

  private groupByRow(seats: BookableSeat[]): SeatRow[] {
    const byRow = new Map<number, BookableSeat[]>();
    for (const seat of seats) {
      const list = byRow.get(seat.row) ?? [];
      list.push(seat);
      byRow.set(seat.row, list);
    }
    return [...byRow.entries()]
      .sort(([a], [b]) => a - b)
      .map(([row, list]) => ({ row, seats: list.sort((a, b) => a.number - b.number) }));
  }

  toggleSeat(seat: BookableSeat): void {
    if (!seat.available || this.submitting) return;
    if (this.selectedSeatIds.has(seat.id)) {
      this.selectedSeatIds.delete(seat.id);
    } else {
      this.selectedSeatIds.add(seat.id);
    }
  }

  get selectedCount(): number {
    return this.selectedSeatIds.size;
  }

  get estimatedTotal(): number {
    let total = 0;
    for (const id of this.selectedSeatIds) {
      const seat = this.seatById.get(id);
      if (seat) total += SEAT_PRICE[seat.seatType] ?? SEAT_PRICE['Standard'];
    }
    return total;
  }

  submit(): void {
    if (this.submitting || !this.selectedScreeningId || this.selectedSeatIds.size === 0) return;

    const userId = this.auth.user()?.id;
    if (!userId) {
      this.error = 'Your session has expired — please sign in again.';
      return;
    }

    this.submitting = true;
    this.error = null;

    let reservationId: string | null = null;

    this.reservationService
      .createReservation({
        screeningId: this.selectedScreeningId,
        seatIds: [...this.selectedSeatIds],
        userId,
      })
      .pipe(
        switchMap((reservation) => {
          reservationId = reservation.id;
          return this.reservationService.pay(reservation.id);
        }),
        switchMap(() => this.reservationService.generateTickets(reservationId!)),
      )
      .subscribe({
        next: () => this.router.navigate(['/reservations', reservationId]),
        error: (err: HttpErrorResponse) => {
          this.submitting = false;
          console.error('Booking failed', err);
          // If the reservation itself was created, send them to it so they can
          // retry payment / cancel; otherwise show the error here.
          if (reservationId) {
            this.router.navigate(['/reservations', reservationId]);
          } else {
            this.error = err.error?.message ?? 'Could not complete the booking.';
          }
        },
      });
  }

  cancel(): void {
    this.router.navigate(['/screenings']);
  }
}
