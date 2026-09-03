import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, forkJoin, of, switchMap } from 'rxjs';


import { CinemaService } from '../../../cinema/services/cinema.service';
import { HallResponse } from '../../../cinema/models/cinema.model';
import { SCREENING_FORMAT_LABELS, Screening } from '../../models/screening.model';
import { ScreeningService } from '../../services/screening.service';
import { MovieService } from '../../../movie/services/movie.service';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-screening-detail',
  standalone: false,
  templateUrl: './screening-detail.component.html',
  styleUrl: './screening-detail.component.css',
})
export class ScreeningDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly screeningService = inject(ScreeningService);
  private readonly movieService = inject(MovieService);
  private readonly cinemaService = inject(CinemaService);
  private readonly auth = inject(AuthService);
  readonly isAdmin = this.auth.isAdmin;
  readonly isLoggedIn = this.auth.isLoggedIn;

  readonly formatLabels = SCREENING_FORMAT_LABELS;

  screening: Screening | null = null;
  movieId: string | null = null
  movieTitle = '';
  coverImage: string | null = null;
  cinemaName = '';
  hallName = '';

  loading = true;
  notFound = false;
  error: string | null = null;
  deleting = false;

  get isPast(): boolean {
    return !!this.screening && new Date(this.screening.startTime).getTime() <= Date.now();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;

    this.screeningService
      .getScreening(id)
      // chain next operations
      .pipe(
        // wait for getScreening and then run forkJoin
        switchMap((screening) => {
          this.screening = screening;
          // run all tree requests in parallel and wait for all
          return forkJoin({
            movie: this.movieService
              .getMovie(screening.movieId)
              .pipe(catchError(() => of(null))),
            cinema: this.cinemaService
              .getCinema(screening.cinemaId)
              .pipe(catchError(() => of(null))),
            halls: this.cinemaService
              .getHallsByCinemaId(screening.cinemaId)
              .pipe(catchError(() => of([] as HallResponse[]))),
          });
        }),
      )
      .subscribe({
        next: ({ movie, cinema, halls }) => {
          this.movieTitle = movie?.title ?? '(unknown movie)';
          this.movieId = movie?.id ?? null;
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
            console.error('GET /screening/{id} failed', err);
            this.error = 'Could not load this screening.';
          }
        },
      });
  }

  delete(): void {
    if (!this.screening || this.deleting) return;
    if (!confirm('Delete this screening? This cannot be undone.')) return;

    this.deleting = true;
    this.error = null;
    this.screeningService.deleteScreening(this.screening.id).subscribe({
      next: () => this.router.navigate(['/screenings']),
      error: (err: HttpErrorResponse) => {
        this.deleting = false;
        console.error('DELETE /screening/{id} failed', err);
        this.error = 'Could not delete this screening.';
      },
    });
  }
}
