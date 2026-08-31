import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { Movie } from '../../../movie/models/movie.model';
import { MovieService } from '../../../movie/services/movie.service';
import { Cinema, HallResponse } from '../../../cinema/models/cinema.model';
import { CinemaService } from '../../../cinema/services/cinema.service';
import {
  ALL_FORMATS,
  SCREENING_FORMAT_LABELS,
  ScreeningFormat,
  ScreeningRequest,
} from '../../models/screening.model';
import { ScreeningService } from '../../services/screening.service';

@Component({
  selector: 'app-screening-form',
  standalone: false,
  templateUrl: './screening-form.component.html',
  styleUrl: './screening-form.component.css',
})
export class ScreeningFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly screeningService = inject(ScreeningService);
  private readonly movieService = inject(MovieService);
  private readonly cinemaService = inject(CinemaService);

  readonly formats = ALL_FORMATS;
  readonly formatLabels = SCREENING_FORMAT_LABELS;

  movies: Movie[] = [];
  cinemas: Cinema[] = [];
  halls: HallResponse[] = [];
  hallsLoading = false;

  screeningId: string | null = null;

  loading = false;
  saving = false;
  error: string | null = null;

  form = this.fb.nonNullable.group({
    movieId: ['', Validators.required],
    cinemaId: ['', Validators.required],
    hallId: [{ value: '', disabled: true }, Validators.required],
    startTime: ['', Validators.required],
    format: [ScreeningFormat.TwoD as ScreeningFormat, Validators.required],
  });

  get isEdit(): boolean {
    return this.screeningId !== null;
  }

  ngOnInit(): void {
    this.movieService.getMovies(1, 200).subscribe({
      next: (res) => (this.movies = res.data),
      error: (err) => console.error('Could not load movies', err),
    });

    this.cinemaService.getCinemas(1, 200).subscribe({
      next: (res) => (this.cinemas = res.data),
      error: (err) => console.error('Could not load cinemas', err),
    });

    // When the cinema changes, the hall that was picked doesn't belong to it anymore
    this.form.controls.cinemaId.valueChanges.subscribe((cinemaId) => {
      this.form.controls.hallId.setValue('');
      if (cinemaId) {
        this.form.controls.hallId.enable();
        this.loadHalls(cinemaId);
      } else {
        this.form.controls.hallId.disable();
      }
    });

    this.screeningId = this.route.snapshot.paramMap.get('id');
    if (this.screeningId) {
      this.loadScreening(this.screeningId);
    }
  }

  private loadScreening(id: string): void {
    this.loading = true;
    this.screeningService.getScreening(id).subscribe({
      next: (screening) => {
        // Patching cinemaId fires its valueChanges handler, which loads the halls.
        this.form.patchValue({
          movieId: screening.movieId,
          cinemaId: screening.cinemaId,
          startTime: screening.startTime.slice(0, 16),
          format: screening.format,
        });
        this.form.controls.hallId.setValue(screening.hallId);
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error =
          err.status === 404 ? 'That screening no longer exists.' : 'Could not load the screening.';
      },
    });
  }

  private loadHalls(cinemaId: string): void {
    if (!cinemaId) {
      this.halls = [];
      return;
    }
    this.hallsLoading = true;
    this.cinemaService.getHallsByCinemaId(cinemaId).subscribe({
      next: (halls) => {
        this.halls = halls;
        this.hallsLoading = false;
      },
      error: (err) => {
        this.hallsLoading = false;
        console.error('Could not load halls', err);
      },
    });
  }

  private toRequest(): ScreeningRequest {
    const v = this.form.getRawValue();
    return {
      movieId: v.movieId,
      cinemaId: v.cinemaId,
      hallId: v.hallId,
      startTime: v.startTime,
      format: v.format,
    };
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.error = null;
    const request = this.toRequest();

    const save$ = this.screeningId
      ? this.screeningService.updateScreening(this.screeningId, request)
      : this.screeningService.createScreening(request);

    save$.subscribe({
      next: (screening) => {
        this.saving = false;
        const id = this.screeningId ?? screening.id;
        this.router.navigate(['/screenings', id]);
      },
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        console.error('Saving screening failed', err);
        this.error = 'Could not save the screening. Check the fields and try again.';
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/screenings']);
  }
}
