import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { ALL_CITIES, CinemaRequest, City } from '../../models/cinema.model';
import { CinemaService } from '../../services/cinema.service';

@Component({
  selector: 'app-cinema-form',
  standalone: false,
  templateUrl: './cinema-form.component.html',
  styleUrl: './cinema-form.component.css',
})
export class CinemaFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly cinemaService = inject(CinemaService);

  readonly cities = ALL_CITIES;

  cinemaId: string | null = null;

  loading = false;
  saving = false;
  error: string | null = null;

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    city: ['' as City | '', Validators.required],
  });

  get isEdit(): boolean {
    return this.cinemaId !== null;
  }

  ngOnInit(): void {
    this.cinemaId = this.route.snapshot.paramMap.get('id');
    if (this.cinemaId) {
      this.loadCinema(this.cinemaId);
    }
  }

  private loadCinema(id: string): void {
    this.loading = true;
    this.cinemaService.getCinema(id).subscribe({
      next: (cinema) => {
        this.form.patchValue({ name: cinema.name, city: cinema.city });
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error =
          err.status === 404 ? 'That cinema no longer exists.' : 'Could not load the cinema.';
      },
    });
  }

  private toRequest(): CinemaRequest {
    const v = this.form.getRawValue();
    return { name: v.name.trim(), city: v.city as City };
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.error = null;
    const request = this.toRequest();

    const save$ = this.cinemaId
      ? this.cinemaService.updateCinema(this.cinemaId, request)
      : this.cinemaService.createCinema(request);

    save$.subscribe({
      next: (cinema) => {
        this.saving = false;
        const id = this.cinemaId ?? cinema.id;
        this.router.navigate(['/cinemas', id]);
      },
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        console.error('Saving cinema failed', err);
        this.error = 'Could not save the cinema. Check the fields and try again.';
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/cinemas']);
  }
}
