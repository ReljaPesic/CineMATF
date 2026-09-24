import { Component, OnInit, inject } from '@angular/core';
import { NonNullableFormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { Cinema } from '../../models/cinema.model';
import { CinemaService } from '../../services/cinema.service';
import { AuthService } from '../../../auth/services/auth.service';
import { RegisterRequest } from '../../../auth/models/auth.model';

@Component({
  selector: 'app-register-cinema-admin',
  standalone: false,
  templateUrl: './register-cinema-admin.component.html',
  styleUrl: './register-cinema-admin.component.css',
})
export class RegisterCinemaAdminComponent implements OnInit {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly cinemaService = inject(CinemaService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  cinemas: Cinema[] = [];
  loadingCinemas = false;
  readonly selectedCinemaIds = new Set<string>();

  readonly form = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    userName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    cardNumber: ['', [Validators.required, Validators.pattern(/^(\d{4}\s?){3}\d{4}$/)]],
    phoneNumber: [''],
  });

  submitting = false;
  error: string | null = null;
  success = false;

  ngOnInit(): void {
    this.loadingCinemas = true;
    // pageSize large enough to cover every cinema in one page for the picker.
    this.cinemaService.getCinemas(1, 100).subscribe({
      next: (res) => {
        this.cinemas = res.data;
        this.loadingCinemas = false;
      },
      error: () => {
        this.loadingCinemas = false;
        this.error = 'Could not load cinemas.';
      },
    });
  }

  toggleCinema(id: string, checked: boolean): void {
    if (checked) {
      this.selectedCinemaIds.add(id);
    } else {
      this.selectedCinemaIds.delete(id);
    }
  }

  submit(): void {
    if (this.submitting) return;

    if (this.form.invalid || this.selectedCinemaIds.size === 0) {
      this.form.markAllAsTouched();
      if (this.selectedCinemaIds.size === 0) {
        this.error = 'Select at least one cinema.';
      }
      return;
    }

    this.submitting = true;
    this.error = null;
    this.success = false;

    const v = this.form.getRawValue();
    const request: RegisterRequest = {
      firstName: v.firstName.trim(),
      lastName: v.lastName.trim(),
      userName: v.userName.trim(),
      email: v.email.trim(),
      password: v.password,
      cardNumber: v.cardNumber.replace(/\s+/g, ''),
      phoneNumber: v.phoneNumber.trim() || null,
      cinemaIds: [...this.selectedCinemaIds],
    };

    this.auth.registerCinemaAdmin(request).subscribe({
      next: () => {
        this.submitting = false;
        this.success = true;
        this.form.reset();
        this.selectedCinemaIds.clear();
      },
      error: (err: HttpErrorResponse) => {
        this.submitting = false;
        this.error =
          err.status === 400
            ? 'Could not create the account — the username or email may already be taken, or the password does not meet the rules.'
            : 'Could not create the account. Is Identity.API running?';
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/cinemas']);
  }
}
