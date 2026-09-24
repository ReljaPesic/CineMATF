import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, forkJoin, map, of } from 'rxjs';

import { Movie } from '../../../movie/models/movie.model';
import { MovieService } from '../../../movie/services/movie.service';
import { Cinema, HallResponse } from '../../../cinema/models/cinema.model';
import { CinemaService } from '../../../cinema/services/cinema.service';
import {
  ALL_FORMATS,
  SCREENING_FORMAT_LABELS,
  Screening,
  ScreeningFormat,
  ScreeningRequest,
} from '../../models/screening.model';
import { ScreeningService } from '../../services/screening.service';

interface ScreeningCreateResult {
  success: boolean;
  startTime: string;
  screening?: Screening;
  message?: string;
}

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
    // Editing an existing screening: single date+time.
    startTime: [''],
    // Creating: a date range (endDate blank = just startDate) + one time-of-day,
    startDate: [''],
    endDate: [''],
    time: ['20:00'],
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
    } else {
      const today = new Date();
      const y = today.getFullYear();
      const m = String(today.getMonth() + 1).padStart(2, '0');
      const d = String(today.getDate()).padStart(2, '0');
      this.form.controls.startDate.setValue(`${y}-${m}-${d}`);
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

  private buildRequests(startTimes: string[]): ScreeningRequest[] {
    const v = this.form.getRawValue();
    return startTimes.map((startTime) => ({
      movieId: v.movieId,
      cinemaId: v.cinemaId,
      hallId: v.hallId,
      format: v.format,
      startTime,
    }));
  }

  // One entry per day from startDate to endDate (inclusive; endDate blank = just
  // startDate), each combined with the chosen time-of-day. Matches what
  // "startTime" (a datetime-local input) sends: "YYYY-MM-DDTHH:mm".
  private buildDateRange(): string[] {
    const v = this.form.getRawValue();
    if (!v.startDate || !v.time) return [];

    const start = new Date(`${v.startDate}T00:00:00`);
    const end = v.endDate ? new Date(`${v.endDate}T00:00:00`) : start;
    if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime()) || end < start) return [];

    const dates: string[] = [];
    const cursor = new Date(start);
    while (cursor <= end) {
      const y = cursor.getFullYear();
      const m = String(cursor.getMonth() + 1).padStart(2, '0');
      const d = String(cursor.getDate()).padStart(2, '0');
      dates.push(`${y}-${m}-${d}T${v.time}`);
      cursor.setDate(cursor.getDate() + 1);
    }
    return dates;
  }

  submit(): void {
    const coreInvalid =
      this.form.controls.movieId.invalid ||
      this.form.controls.cinemaId.invalid ||
      this.form.controls.hallId.invalid ||
      this.form.controls.format.invalid;

    if (this.isEdit) {
      if (coreInvalid || !this.form.controls.startTime.value) {
        this.form.markAllAsTouched();
        return;
      }

      this.saving = true;
      this.error = null;
      this.screeningService.updateScreening(this.screeningId!, this.toRequest()).subscribe({
        next: (screening) => {
          this.saving = false;
          this.router.navigate(['/screenings', screening.id]);
        },
        error: (err: HttpErrorResponse) => {
          this.saving = false;
          console.error('Saving screening failed', err);
          this.error = 'Could not save the screening. Check the fields and try again.';
        },
      });
      return;
    }

    const startTimes = this.buildDateRange();
    if (coreInvalid || startTimes.length === 0) {
      this.form.markAllAsTouched();
      if (startTimes.length === 0) {
        this.error = 'Pick a start date (and a time) - the end date must be on or after the start date.';
      }
      return;
    }

    this.saving = true;
    this.error = null;

    // Each request reports its own success/failure (instead of one failing request
    // aborting the whole batch), so a single occupied day doesn't lose the others.
    const requests = this.buildRequests(startTimes).map((request) =>
      this.screeningService.createScreening(request).pipe(
        map((screening): ScreeningCreateResult => ({ success: true, startTime: request.startTime, screening })),
        catchError((err: HttpErrorResponse) =>
          of<ScreeningCreateResult>({
            success: false,
            startTime: request.startTime,
            message: err.error?.message ?? 'Could not create this screening.',
          }),
        ),
      ),
    );

    forkJoin(requests).subscribe((results) => {
      this.saving = false;
      const succeeded = results.filter((r) => r.success);
      const failed = results.filter((r) => !r.success);

      if (failed.length === 0) {
        if (succeeded.length === 1) {
          this.router.navigate(['/screenings', succeeded[0].screening!.id]);
        } else {
          this.router.navigate(['/screenings']);
        }
        return;
      }

      const failedList = failed.map((f) => `${f.startTime.replace('T', ' ')} — ${f.message}`).join('; ');
      this.error =
        succeeded.length === 0
          ? `Could not create any screenings. ${failedList}`
          : `Created ${succeeded.length} of ${results.length} screenings. Failed: ${failedList}`;
    });
  }

  cancel(): void {
    this.router.navigate(['/screenings']);
  }
}
