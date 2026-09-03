import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of, switchMap } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { MovieService } from '../../../movie/services/movie.service';
import { CinemaService } from '../../../cinema/services/cinema.service';
import { ALL_SEAT_TYPES, SeatResponse } from '../../../cinema/models/cinema.model';
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

  readonly seatTypes = ALL_SEAT_TYPES;

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

      const movieId = params.get('movieId');
      if (movieId) {
        this.screenings = this.allFutureScreenings.filter((s) => s.movieId === movieId);
        this.filterMovieTitle = this.movieTitles.get(movieId) ?? null;
      }

      this.loadingScreenings = false;
      const preselect = params.get('screeningId');
      if (preselect) {
        if (this.screenings.some((s) => s.id === preselect)) {
          this.selectedScreeningId = preselect;
          this.onScreeningChange();
        } else {
          // Don't silently fall back to some other, unrelated screening
          // (e.g. the only one left after filtering out past screenings) -
          // that would let the user book a different movie than they picked.
          this.error = 'That screening is no longer available for booking.';
        }
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

    // Switching screenings again before this request settles must not let a
    // late/out-of-order response for the *previous* screening overwrite the
    // seat map for the one the user is now looking at.
    const requestedScreeningId = this.selectedScreeningId;

    this.loadingSeats = true;
    this.screeningService
      .getScreening(requestedScreeningId)
      .pipe(
        switchMap((screening) => {
          if (requestedScreeningId !== this.selectedScreeningId) return of(null);
          this.screening = screening;
          return forkJoin({
            available: this.reservationService.getAvailableSeats(screening.id),
            seats: this.cinemaService.getSeatsByCinemaAndHallIds(screening.cinemaId, screening.hallId),
          });
        }),
      )
      .subscribe({
        next: (result) => {
          if (!result || requestedScreeningId !== this.selectedScreeningId) return;
          const { available, seats } = result;
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
