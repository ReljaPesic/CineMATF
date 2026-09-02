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

  ngOnInit(): void {
    forkJoin({
      screenings: this.screeningService.getScreenings().pipe(catchError(() => of([] as Screening[]))),
      movies: this.movieService.getMovies(1, 100).pipe(catchError(() => of(null))),
    }).subscribe(({ screenings, movies }) => {
      this.movieTitles = new Map((movies?.data ?? []).map((m) => [m.id, m.title]));
      this.screenings = [...screenings]
        .filter((s) => new Date(s.startTime).getTime() > Date.now())
        .sort((a, b) => a.startTime.localeCompare(b.startTime));
      this.loadingScreenings = false;

      const preselect = this.route.snapshot.queryParamMap.get('screeningId');
      if (preselect && this.screenings.some((s) => s.id === preselect)) {
        this.selectedScreeningId = preselect;
        this.onScreeningChange();
      }
    });
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

    this.loadingSeats = true;
    this.screeningService
      .getScreening(this.selectedScreeningId)
      .pipe(
        switchMap((screening) => {
          this.screening = screening;
          return forkJoin({
            available: this.reservationService.getAvailableSeats(screening.id),
            seats: this.cinemaService.getSeatsByCinemaAndHallIds(screening.cinemaId, screening.hallId),
          });
        }),
      )
      .subscribe({
        next: ({ available, seats }) => {
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
    this.reservationService
      .createReservation({
        screeningId: this.selectedScreeningId,
        seatIds: [...this.selectedSeatIds],
        userId,
      })
      .subscribe({
        next: (reservation) => this.router.navigate(['/reservations', reservation.id]),
        error: (err: HttpErrorResponse) => {
          this.submitting = false;
          console.error('POST /reservations failed', err);
          this.error = err.error?.message ?? 'Could not create the reservation.';
        },
      });
  }

  cancel(): void {
    this.router.navigate(['/reservations']);
  }
}
