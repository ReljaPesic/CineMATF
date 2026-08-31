import { Component, OnInit, inject } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { ALL_GENRES, Genre, Movie, MovieRequest } from '../../models/movie.model';
import { MovieService } from '../../services/movie.service';


@Component({
  selector: 'app-movie-form',
  standalone: false,
  templateUrl: './movie-form.component.html',
  styleUrl: './movie-form.component.css',
})
export class MovieFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly movieService = inject(MovieService);

  readonly genres = ALL_GENRES;

  movieId: string | null = null;

  loading = false;
  saving = false;
  error: string | null = null;

  form = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    description: ['', [Validators.required, Validators.maxLength(2000)]],
    durationMinutes: [90, [Validators.required, Validators.min(1), Validators.max(1000)]],
    releaseDate: ['', Validators.required],
    rating: [7, [Validators.required, Validators.min(0), Validators.max(10)]],
    coverImage: [''],
    genres: [[] as Genre[], Validators.required],
    actors: this.fb.array<FormGroup>([], [Validators.required]),
  });

  get actors(): FormArray<FormGroup> {
    return this.form.controls.actors;
  }

  get isEdit(): boolean {
    return this.movieId !== null;
  }

  ngOnInit(): void {
    this.movieId = this.route.snapshot.paramMap.get('id');

    if (this.movieId) {
      this.loadMovie(this.movieId);
    } else {
      this.addActor();
    }
  }

  private loadMovie(id: string): void {
    this.loading = true;
    this.movieService.getMovie(id).subscribe({
      next: (movie) => {
        this.patchForm(movie);
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error =
          err.status === 404 ? 'That movie no longer exists.' : 'Could not load the movie.';
      },
    });
  }

  private patchForm(movie: Movie): void {
    this.form.patchValue({
      title: movie.title,
      description: movie.description,
      durationMinutes: movie.durationMinutes,
      releaseDate: movie.releaseDate?.slice(0, 10) ?? '',
      rating: movie.rating,
      coverImage: movie.coverImage ?? '',
      genres: [...movie.genres],
    });

    this.actors.clear();
    if (movie.actors.length === 0) {
      this.addActor();
    } else {
      movie.actors.forEach((a) => this.actors.push(this.actorGroup(a.firstName, a.lastName)));
    }
  }

  private actorGroup(firstName = '', lastName = ''): FormGroup {
    return this.fb.group({
      firstName: [firstName, Validators.required],
      lastName: [lastName, Validators.required],
    });
  }

  addActor(): void {
    this.actors.push(this.actorGroup());
  }

  removeActor(index: number): void {
    this.actors.removeAt(index);
  }

  isGenreSelected(genre: Genre): boolean {
    return this.form.controls.genres.value!.includes(genre);
  }

  toggleGenre(genre: Genre): void {
    const current = this.form.controls.genres.value!;
    const next = current.includes(genre)
      ? current.filter((g) => g !== genre)
      : [...current, genre];
    this.form.controls.genres.setValue(next);
    this.form.controls.genres.markAsDirty();
  }

  private toRequest(): MovieRequest {
    const v = this.form.getRawValue();
    return {
      title: v.title!.trim(),
      description: v.description!.trim(),
      durationMinutes: v.durationMinutes!,
      releaseDate: v.releaseDate!,
      rating: v.rating!,
      coverImage: v.coverImage?.trim() || null,
      genres: v.genres!,
      actors: v.actors.map((a) => ({ firstName: a['firstName'], lastName: a['lastName'] })),
    };
  }

  get coverPreview(): string | null {
    return this.form.controls.coverImage.value?.trim() || null;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.error = null;
    const request = this.toRequest();

    const save$ = this.movieId
      ? this.movieService.updateMovie(this.movieId, request)
      : this.movieService.createMovie(request);

    save$.subscribe({
      next: (movie) => {
        this.saving = false;
        const id = this.movieId ?? movie.id;
        this.router.navigate(['/movies', id]);
      },
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        console.error('Saving movie failed', err);
        this.error = 'Could not save the movie. Check the fields and try again.';
      },
    });
  }

  cancel(): void {
    this.router.navigate(this.movieId ? ['/movies', this.movieId] : ['/movies']);
  }
}
