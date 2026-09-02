import { Component, OnInit, inject } from '@angular/core';


import { Cinema } from '../../../cinema/models/cinema.model';
import { CinemaService } from '../../../cinema/services/cinema.service';
import { SCREENING_FORMAT_LABELS, Screening, ScreeningFilter } from '../../models/screening.model';
import { ScreeningService } from '../../services/screening.service';
import { MovieService } from '../../../movie/services/movie.service';
import { Movie } from '../../../movie/models/movie.model';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-screening-list',
  standalone: false,
  templateUrl: './screening-list.component.html',
  styleUrl: './screening-list.component.css',
})
export class ScreeningListComponent implements OnInit {
  private readonly screeningService = inject(ScreeningService);
  private readonly movieService = inject(MovieService);
  private readonly cinemaService = inject(CinemaService);
  readonly isAdmin = inject(AuthService).isAdmin;

  screenings: Screening[] = [];
  movies: Movie[] = [];
  cinemas: Cinema[] = [];

  private movieTitles = new Map<string, string>();
  private cinemaNames = new Map<string, string>();

  readonly formatLabels = SCREENING_FORMAT_LABELS;

  selectedMovieId = '';
  selectedCinemaId = '';
  selectedDate = '';

  private filter: ScreeningFilter = {};

  loading = false;
  error: string | null = null;
  deletingId: string | null = null;

  get hasFilter(): boolean {
    return !!(this.selectedMovieId || this.selectedCinemaId || this.selectedDate);
  }

  ngOnInit(): void {
    this.movieService.getMovies(1, 10).subscribe({
      next: (res) => {
        this.movies = res.data;
        this.movieTitles = new Map(res.data.map((m) => [m.id, m.title]));
      },
      error: (err) => console.error('Could not load movies for filter', err),
    });

    this.cinemaService.getCinemas(1, 10).subscribe({
      next: (res) => {
        this.cinemas = res.data;
        this.cinemaNames = new Map(res.data.map((c) => [c.id, c.name]));
      },
      error: (err) => console.error('Could not load cinemas for filter', err),
    });

    this.load();
  }

  applyFilters(): void {
    this.filter = {
      movieId: this.selectedMovieId || undefined,
      cinemaId: this.selectedCinemaId || undefined,
      date: this.selectedDate || undefined,
    };
    this.load();
  }

  clearFilters(): void {
    this.selectedMovieId = '';
    this.selectedCinemaId = '';
    this.selectedDate = '';
    this.filter = {};
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;
    this.screeningService.getScreenings(this.filter).subscribe({
      next: (screenings) => {
        this.screenings = [...screenings].sort((a, b) => a.startTime.localeCompare(b.startTime));
        this.loading = false;
      },
      error: (err) => {
        console.error('GET /screening failed', err);
        this.error = 'Could not load screenings. Is Screening.API running on :8080?';
        this.screenings = [];
        this.loading = false;
      },
    });
  }

  getMovieTitle(id: string): string {
    return this.movieTitles.get(id) ?? '(unknown movie)';
  }

  getCinemaName(id: string): string {
    return this.cinemaNames.get(id) ?? '(unknown cinema)';
  }

  delete(screening: Screening): void {
    if (this.deletingId) return;
    if (!confirm('Delete this screening? This cannot be undone.')) return;

    this.deletingId = screening.id;
    this.screeningService.deleteScreening(screening.id).subscribe({
      next: () => {
        this.deletingId = null;
        this.load();
      },
      error: (err) => {
        this.deletingId = null;
        console.error('DELETE /screening failed', err);
        this.error = 'Could not delete this screening.';
      },
    });
  }
}
