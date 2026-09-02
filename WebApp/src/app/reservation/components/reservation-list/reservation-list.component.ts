import { Component, OnInit, inject } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { MovieService } from '../../../movie/services/movie.service';
import { CinemaService } from '../../../cinema/services/cinema.service';
import { ScreeningService } from '../../../screening/services/screening.service';
import { Reservation, ReservationStatus } from '../../models/reservation.model';
import { ReservationService } from '../../services/reservation.service';

@Component({
  selector: 'app-reservation-list',
  standalone: false,
  templateUrl: './reservation-list.component.html',
  styleUrl: './reservation-list.component.css',
})
export class ReservationListComponent implements OnInit {
  private readonly reservationService = inject(ReservationService);
  private readonly screeningService = inject(ScreeningService);
  private readonly movieService = inject(MovieService);
  private readonly cinemaService = inject(CinemaService);

  reservations: Reservation[] = [];

  // screeningId -> "Movie title — Cinema" 
  private screeningLabels = new Map<string, string>();

  readonly statuses = Object.values(ReservationStatus);
  selectedStatus = '';

  loading = true;
  error: string | null = null;

  get visibleReservations(): Reservation[] {
    if (!this.selectedStatus) return this.reservations;
    return this.reservations.filter((r) => r.status === this.selectedStatus);
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;

    this.reservationService.getReservations().subscribe({
      next: (reservations) => {
        this.reservations = reservations
        this.loading = false;
        this.loadScreeningLabels();
      },
      error: (err) => {
        console.error('GET /reservations failed', err);
        this.error = 'Could not load reservations. Is Reservation.API running?';
        this.reservations = [];
        this.loading = false;
      },
    });
  }


  private loadScreeningLabels(): void {
    const screeningIds = [...new Set(this.reservations.map((r) => r.screeningId))];
    if (screeningIds.length === 0) return;

    forkJoin({
      screenings: forkJoin(
        screeningIds.map((id) =>
          this.screeningService.getScreening(id).pipe(catchError(() => of(null))),
        ),
      ),
      movies: this.movieService.getMovies(1, 100).pipe(catchError(() => of(null))),
      cinemas: this.cinemaService.getCinemas(1, 100).pipe(catchError(() => of(null))),
    }).subscribe(({ screenings, movies, cinemas }) => {
      const movieTitles = new Map((movies?.data ?? []).map((m) => [m.id, m.title]));
      const cinemaNames = new Map((cinemas?.data ?? []).map((c) => [c.id, c.name]));

      for (const screening of screenings) {
        if (!screening) continue;
        const title = movieTitles.get(screening.movieId) ?? '(unknown movie)';
        const cinema = cinemaNames.get(screening.cinemaId) ?? '(unknown cinema)';
        this.screeningLabels.set(screening.id, `${title} — ${cinema}`);
      }
    });
  }

  screeningLabel(screeningId: string): string {
    return this.screeningLabels.get(screeningId) ?? '…';
  }

  seatCount(reservation: Reservation): number {
    return reservation.tickets.length;
  }
}
