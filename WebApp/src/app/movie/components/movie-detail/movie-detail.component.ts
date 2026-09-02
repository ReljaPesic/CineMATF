import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { switchMap } from 'rxjs';

import { Movie } from '../../models/movie.model';
import { MovieService } from '../../services/movie.service';


@Component({
  selector: 'app-movie-detail',
  standalone: false,
  templateUrl: './movie-detail.component.html',
  styleUrl: './movie-detail.component.css',
})
export class MovieDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly movieService = inject(MovieService);

  movie: Movie | null = null;
  loading = true;
  notFound = false;
  error: string | null = null;
  deleting = false;

  ngOnInit(): void {
    // route.paramMap is an Observable: it re-emits if the user navigates from
    // /movies/A straight to /movies/B without leaving this component. switchMap
    // cancels the in-flight request for A and starts the one for B.
    this.route.paramMap
      .pipe(
        switchMap((params) => {
          this.loading = true;
          this.notFound = false;
          this.error = null;
          const id = params.get('id')!;
          return this.movieService.getMovie(id);
        }),
      )
      .subscribe({
        next: (movie) => {
          this.movie = movie;
          this.loading = false;
        },
        error: (err: HttpErrorResponse) => {
          this.loading = false;
          if (err.status === 404) {
            this.notFound = true;
          } else {
            console.error('GET /movie/{id} failed', err);
            this.error = 'Could not load this movie.';
          }
        },
      });
  }

  /** DELETE /movie/{id}, then go back to the list. */
  delete(): void {
    if (!this.movie || this.deleting) return;
    if (!confirm(`Delete "${this.movie.title}"? This cannot be undone.`)) return;

    this.deleting = true;
    this.error = null;
    this.movieService.deleteMovie(this.movie.id).subscribe({
      next: () => this.router.navigate(['/movies']),
      error: (err: HttpErrorResponse) => {
        this.deleting = false;
        console.error('DELETE /movie/{id} failed', err);
        this.error = 'Could not delete this movie.';
      },
    });
  }
}
