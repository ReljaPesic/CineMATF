import { Component, OnInit, inject } from '@angular/core';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { ALL_GENRES, Genre, Movie } from '../../models/movie.model';
import { MovieService } from '../../services/movie.service';
import { PagedResponse } from '../../../shared/models/shared.models';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-movie-list',
  standalone: false,
  templateUrl: './movie-list.component.html',
  styleUrl: './movie-list.component.css',
})
export class MovieListComponent implements OnInit {

  private readonly movieService = inject(MovieService);
  readonly isAdmin = inject(AuthService).isAdmin;

  movies: Movie[] = [];

  page = 1;
  pageSize = 10;
  totalCount = 0;

  paged = true;


  readonly searchControl = new FormControl('', { nonNullable: true });
  readonly genres = ALL_GENRES;
  selectedGenre: Genre | '' = '';

  loading = false;
  error: string | null = null;

  deletingId: string | null = null;

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((term) => {
        if (term.trim() && this.selectedGenre) {
          this.selectedGenre = '';
        }
        this.page = 1;
        this.load();
      });

    this.load();
  }


  onGenreChange(): void {
    if (this.selectedGenre) {
      this.searchControl.setValue('', { emitEvent: false });
    }
    this.page = 1;
    this.load();
  }

  clearFilters(): void {
    this.selectedGenre = '';
    this.searchControl.setValue('', { emitEvent: false });
    this.page = 1;
    this.load();
  }

  get hasFilter(): boolean {
    return !!this.searchControl.value.trim() || !!this.selectedGenre;
  }

  load(): void {
    this.loading = true;
    this.error = null;

    const term = this.searchControl.value.trim();

    if (term) {
      this.paged = false;
      this.movieService.search(term).subscribe({
        next: (movies) => this.showList(movies),
        error: (err) => this.showError('Search failed.', err),
      });
      return;
    }

    if (this.selectedGenre) {
      this.paged = false;
      this.movieService.getByGenre(this.selectedGenre).subscribe({
        next: (movies) => this.showList(movies),
        error: (err) => this.showError('Could not filter by genre.', err),
      });
      return;
    }

    this.paged = true;
    this.movieService.getMovies(this.page, this.pageSize).subscribe({
      next: (res: PagedResponse<Movie>) => {
        this.movies = res.data;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err) => this.showError('Could not load movies. Is Movie.API running on :5011?', err),
    });
  }


  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.page) {
      return;
    }
    this.page = page;
    this.load();
  }


  delete(movie: Movie): void {
    if (this.deletingId) return;
    if (!confirm(`Delete "${movie.title}"? This cannot be undone.`)) return;

    this.deletingId = movie.id;
    this.movieService.deleteMovie(movie.id).subscribe({
      next: () => {
        this.deletingId = null;
        if (this.paged && this.movies.length === 1 && this.page > 1) {
          this.page--;
        }
        this.load();
      },
      error: (err) => {
        this.deletingId = null;
        console.error('DELETE /movie failed', err);
        this.error = `Could not delete "${movie.title}".`;
      },
    });
  }

  private showList(movies: Movie[]): void {
    this.movies = movies;
    this.totalCount = movies.length;
    this.loading = false;
  }

  private showError(message: string, err: unknown): void {
    console.error(message, err);
    this.error = message;
    this.movies = [];
    this.loading = false;
  }
}
