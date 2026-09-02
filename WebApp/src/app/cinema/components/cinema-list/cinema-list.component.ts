import { Component, OnInit, inject } from '@angular/core';

import { PagedResponse } from '../../../shared/models/shared.models';
import { ALL_CITIES, Cinema, City } from '../../models/cinema.model';
import { CinemaService } from '../../services/cinema.service';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-cinema-list',
  standalone: false,
  templateUrl: './cinema-list.component.html',
  styleUrl: './cinema-list.component.css',
})
export class CinemaListComponent implements OnInit {
  private readonly cinemaService = inject(CinemaService);
  readonly isAdmin = inject(AuthService).isAdmin;

  cinemas: Cinema[] = [];

  page = 1;
  pageSize = 10;
  totalCount = 0;

  paged = true;

  readonly cities = ALL_CITIES;
  selectedCity: City | '' = '';

  loading = false;
  error: string | null = null;
  deletingId: string | null = null;

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  get hasFilter(): boolean {
    return !!this.selectedCity;
  }

  ngOnInit(): void {
    this.load();
  }

  onCityChange(): void {
    this.page = 1;
    this.load();
  }

  clearFilters(): void {
    this.selectedCity = '';
    this.page = 1;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;

    if (this.selectedCity) {
      this.paged = false;
      this.cinemaService.getCinemaByCity(this.selectedCity).subscribe({
        next: (cinemas) => this.showList(cinemas),
        error: (err) => this.showError('Could not filter by city.', err),
      });
      return;
    }

    this.paged = true;
    this.cinemaService.getCinemas(this.page, this.pageSize).subscribe({
      next: (res: PagedResponse<Cinema>) => {
        this.cinemas = res.data;
        this.totalCount = res.totalCount;
        this.loading = false;
      },
      error: (err) =>
        this.showError('Could not load cinemas. Is Cinema.API running on :5000?', err),
    });
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.page) return;
    this.page = page;
    this.load();
  }

  delete(cinema: Cinema): void {
    if (this.deletingId) return;
    if (!confirm(`Delete "${cinema.name}"? This also removes its halls and seats.`)) return;

    this.deletingId = cinema.id;
    this.cinemaService.deleteCinema(cinema.id).subscribe({
      next: () => {
        this.deletingId = null;
        if (this.paged && this.cinemas.length === 1 && this.page > 1) {
          this.page--;
        }
        this.load();
      },
      error: (err) => {
        this.deletingId = null;
        console.error('DELETE /cinema failed', err);
        this.error = `Could not delete "${cinema.name}".`;
      },
    });
  }

  private showList(cinemas: Cinema[]): void {
    this.cinemas = cinemas;
    this.totalCount = cinemas.length;
    this.loading = false;
  }

  private showError(message: string, err: unknown): void {
    console.error(message, err);
    this.error = message;
    this.cinemas = [];
    this.loading = false;
  }
}
