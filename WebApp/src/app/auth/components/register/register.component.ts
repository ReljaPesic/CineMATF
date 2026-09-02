import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  form = {
    firstName: '',
    lastName: '',
    userName: '',
    email: '',
    password: '',
    cardNumber: '',
    phoneNumber: '',
  };

  submitting = false;
  error: string | null = null;

  submit(): void {
    if (this.submitting) return;
    this.submitting = true;
    this.error = null;

    const request = {
      firstName: this.form.firstName.trim(),
      lastName: this.form.lastName.trim(),
      userName: this.form.userName.trim(),
      email: this.form.email.trim(),
      password: this.form.password,
      cardNumber: this.form.cardNumber.trim(),
      phoneNumber: this.form.phoneNumber.trim() || null,
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
