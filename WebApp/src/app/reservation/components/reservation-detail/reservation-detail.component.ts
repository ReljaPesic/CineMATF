import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, forkJoin, of, switchMap } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { MovieService } from '../../../movie/services/movie.service';
import { CinemaService } from '../../../cinema/services/cinema.service';
import { HallResponse } from '../../../cinema/models/cinema.model';
import { ScreeningService } from '../../../screening/services/screening.service';
import { SCREENING_FORMAT_LABELS, Screening } from '../../../screening/models/screening.model';
import { Reservation, ReservationStatus } from '../../models/reservation.model';
import { ReservationService } from '../../services/reservation.service';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-reservation-detail',
  standalone: false,
  templateUrl: './reservation-detail.component.html',
  styleUrl: './reservation-detail.component.css',
})
export class ReservationDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly reservationService = inject(ReservationService);
  private readonly screeningService = inject(ScreeningService);
  private readonly movieService = inject(MovieService);
  private readonly cinemaService = inject(CinemaService);
  readonly isAdmin = inject(AuthService).isAdmin;

  readonly formatLabels = SCREENING_FORMAT_LABELS;
  readonly Status = ReservationStatus;

  // Non-admins have no reservations list to go back to.
  get backLink(): string {
    return this.isAdmin() ? '/reservations' : '/screenings';
  }

  reservation: Reservation | null = null;
  screening: Screening | null = null;
  movieId: string | null = null;
  movieTitle = '';
  coverImage: string | null = null;
  cinemaName = '';
  hallName = '';

  loading = true;
  notFound = false;
  error: string | null = null;
  working = false;

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.loading = true;
    this.error = null;

    this.reservationService
      .getReservation(id)
      .pipe(
        switchMap((reservation) => {
          this.reservation = reservation;
          return this.screeningService
            .getScreening(reservation.screeningId)
            .pipe(catchError(() => of(null)));
        }),
        switchMap((screening) => {
          this.screening = screening;
          if (!screening) {
            return of({ movie: null, cinema: null, halls: [] as HallResponse[] });
          }
          return forkJoin({
            movie: this.movieService.getMovie(screening.movieId).pipe(catchError(() => of(null))),
            cinema: this.cinemaService.getCinema(screening.cinemaId).pipe(catchError(() => of(null))),
            halls: this.cinemaService
              .getHallsByCinemaId(screening.cinemaId)
              .pipe(catchError(() => of([] as HallResponse[]))),
          });
        }),
      )
      .subscribe({
        next: ({ movie, cinema, halls }) => {
          this.movieId = movie?.id ?? null;
          this.movieTitle = movie?.title ?? '(unknown movie)';
          this.coverImage = movie?.coverImage ?? null;
          this.cinemaName = cinema?.name ?? '(unknown cinema)';
          this.hallName =
            halls.find((h) => h.id === this.screening?.hallId)?.name ?? '(unknown hall)';
          this.loading = false;
        },
        error: (err: HttpErrorResponse) => {
          this.loading = false;
          if (err.status === 404) {
            this.notFound = true;
          } else {
            console.error('GET /reservations/{id} failed', err);
            this.error = 'Could not load this reservation.';
          }
        },
      });
  }

  pay(): void {
    if (!this.reservation || this.working) return;
    this.run(this.reservationService.pay(this.reservation.id), 'Could not complete the payment.');
  }

  cancel(): void {
    if (!this.reservation || this.working) return;
    if (!confirm('Cancel this reservation? The held seats will be released.')) return;
    this.run(this.reservationService.cancel(this.reservation.id), 'Could not cancel this reservation.');
  }

  generateTickets(): void {
    if (!this.reservation || this.working) return;
    this.run(
      this.reservationService.generateTickets(this.reservation.id),
      'Could not generate tickets.',
    );
  }

  private run(action: Observable<unknown>, failure: string): void {
    this.working = true;
    this.error = null;
    action.subscribe({
      next: () => {
        this.working = false;
        this.load();
      },
      error: (err: HttpErrorResponse) => {
        this.working = false;
        this.error = err.error?.message ?? failure;
      },
    });
  }

  downloadingTicketId: string | null = null;

  downloadTicket(ticketId: string): void {
    if (this.downloadingTicketId) return;
    this.downloadingTicketId = ticketId;
    this.reservationService.downloadTicket(ticketId).subscribe({
      next: (response) => {
        this.downloadingTicketId = null;
        const blob = response.body;
        if (!blob) return;
        const fileName = this.extractFileName(response.headers.get('content-disposition')) ?? `ticket-${ticketId}.pdf`;
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        URL.revokeObjectURL(url);
      },
      error: (err: HttpErrorResponse) => {
        this.downloadingTicketId = null;
        console.error('GET /Ticket/{id}/download failed', err);
        this.error = 'Could not download this ticket.';
      },
    });
  }

  private extractFileName(contentDisposition: string | null): string | null {
    if (!contentDisposition) return null;
    const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(contentDisposition);
    return match ? decodeURIComponent(match[1]) : null;
  }

  back(): void {
    this.router.navigateByUrl(this.backLink);
  }
}
