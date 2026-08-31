import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { Cinema, HallRequest, HallResponse } from '../../models/cinema.model';
import { CinemaService } from '../../services/cinema.service';

@Component({
  selector: 'app-cinema-detail',
  standalone: false,
  templateUrl: './cinema-detail.component.html',
  styleUrl: './cinema-detail.component.css',
})
export class CinemaDetailComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly cinemaService = inject(CinemaService);

  cinemaId!: string;
  cinema: Cinema | null = null;
  halls: HallResponse[] = [];

  loading = true;
  notFound = false;
  error: string | null = null;
  deleting = false;

  hallsLoading = false;
  hallsError: string | null = null;
  addingHall = false;
  deletingHallId: string | null = null;

  hallForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(50)]],
    totalRows: [10, [Validators.required, Validators.min(1), Validators.max(100)]],
    seatsPerRow: [12, [Validators.required, Validators.min(1), Validators.max(50)]],
  });

  ngOnInit(): void {
    this.cinemaId = this.route.snapshot.paramMap.get('id')!;
    this.loadCinema();
    this.loadHalls();
  }

  private loadCinema(): void {
    this.loading = true;
    this.notFound = false;
    this.error = null;
    this.cinemaService.getCinema(this.cinemaId).subscribe({
      next: (cinema) => {
        this.cinema = cinema;
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        if (err.status === 404) {
          this.notFound = true;
        } else {
          console.error('GET /cinema/{id} failed', err);
          this.error = 'Could not load this cinema.';
        }
      },
    });
  }

  private loadHalls(): void {
    this.hallsLoading = true;
    this.hallsError = null;
    this.cinemaService.getHallsByCinemaId(this.cinemaId).subscribe({
      next: (halls) => {
        this.halls = halls;
        this.hallsLoading = false;
      },
      error: (err) => {
        this.hallsLoading = false;
        console.error('GET /cinema/{id}/halls failed', err);
        this.hallsError = 'Could not load halls.';
      },
    });
  }

  seatCount(hall: HallResponse): number {
    return hall.totalRows * hall.seatsPerRow;
  }

  addHall(): void {
    if (this.hallForm.invalid) {
      this.hallForm.markAllAsTouched();
      return;
    }

    this.addingHall = true;
    this.hallsError = null;
    const v = this.hallForm.getRawValue();
    const request: HallRequest = {
      name: v.name.trim(),
      totalRows: v.totalRows,
      seatsPerRow: v.seatsPerRow,
    };

    this.cinemaService.createHalls(this.cinemaId, { halls: [request] }).subscribe({
      next: (res) => {
        this.addingHall = false;
        if (res.created === 0) {
          const reason = res.failed[0]?.error;
          this.hallsError = reason
            ? `Could not add "${request.name}": ${reason}.`
            : `Could not add "${request.name}".`;
          return;
        }
        this.hallForm.reset({ name: '', totalRows: 10, seatsPerRow: 12 });
        this.loadHalls();
      },
      error: (err) => {
        this.addingHall = false;
        console.error('POST /cinema/{id}/halls failed', err);
        this.hallsError = 'Could not add the hall.';
      },
    });
  }

  deleteHall(hall: HallResponse): void {
    if (this.deletingHallId) return;
    if (!confirm(`Delete hall "${hall.name}" and its seats?`)) return;

    this.deletingHallId = hall.id;
    this.cinemaService.deleteHallByCinemaAndHallId(this.cinemaId, hall.id).subscribe({
      next: () => {
        this.deletingHallId = null;
        this.loadHalls();
      },
      error: (err) => {
        this.deletingHallId = null;
        console.error('DELETE hall failed', err);
        this.hallsError = `Could not delete "${hall.name}".`;
      },
    });
  }

  delete(): void {
    if (!this.cinema || this.deleting) return;
    if (!confirm(`Delete "${this.cinema.name}"? This also removes its halls and seats.`)) return;

    this.deleting = true;
    this.error = null;
    this.cinemaService.deleteCinema(this.cinema.id).subscribe({
      next: () => this.router.navigate(['/cinemas']),
      error: (err: HttpErrorResponse) => {
        this.deleting = false;
        console.error('DELETE /cinema/{id} failed', err);
        this.error = 'Could not delete this cinema.';
      },
    });
  }
}
