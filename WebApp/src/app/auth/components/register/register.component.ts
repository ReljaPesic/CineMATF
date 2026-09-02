import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  NonNullableFormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../services/auth.service';


export function passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
  const password = group.get('password')?.value;
  const confirmPassword = group.get('confirmPassword')?.value;
  return password === confirmPassword ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  private readonly fb = inject(NonNullableFormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly registerForm = this.fb.group(
    {
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      userName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      cardNumber: ['', [Validators.required, Validators.pattern(/^(\d{4}\s?){3}\d{4}$/)]],
      phoneNumber: [''],
    },
    { validators: passwordMatchValidator, updateOn: 'blur' },
  );

  submitting = false;
  error: string | null = null;


  get firstName(): AbstractControl | null {
    return this.registerForm.get('firstName');
  }
  get lastName(): AbstractControl {
    return this.registerForm.controls.lastName;
  }
  get userName(): AbstractControl {
    return this.registerForm.controls.userName;
  }
  get email(): AbstractControl {
    return this.registerForm.controls.email;
  }
  get password(): AbstractControl {
    return this.registerForm.controls.password;
  }
  get confirmPassword(): AbstractControl {
    return this.registerForm.controls.confirmPassword;
  }
  get cardNumber(): AbstractControl {
    return this.registerForm.controls.cardNumber;
  }

  submit(): void {
    if (this.submitting) return;

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.error = null;

    const v = this.registerForm.getRawValue();
    const request = {
      firstName: v.firstName.trim(),
      lastName: v.lastName.trim(),
      userName: v.userName.trim(),
      email: v.email.trim(),
      password: v.password,
      cardNumber: v.cardNumber.replace(/\s+/g, ''),
      phoneNumber: v.phoneNumber.trim() || null,
    };

    this.auth.register(request).subscribe({
      next: () => {
        // Sign the new account straight in, then land on the app.
        this.auth.login({ userName: request.userName, password: request.password }).subscribe({
          next: () => this.router.navigateByUrl('/screenings'),
          error: () => this.router.navigateByUrl('/login'),
        });
      },
      error: (err: HttpErrorResponse) => {
        this.submitting = false;
        this.error =
          err.status === 400
            ? 'Could not register — the username or email may already be taken, or the password does not meet the rules.'
            : 'Could not register. Is Identity.API running?';
      },
    });
  }
}
