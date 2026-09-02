import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  userName = '';
  password = '';
  submitting = false;
  error: string | null = null;

  submit(): void {
    if (this.submitting || !this.userName || !this.password) return;

    this.submitting = true;
    this.error = null;

    this.auth.login({ userName: this.userName, password: this.password }).subscribe({
      next: () => {
        const returnUrl = '/screenings';
        this.router.navigateByUrl(returnUrl);
      },
      error: (err: HttpErrorResponse) => {
        this.submitting = false;
        this.error =
          err.status === 401
            ? 'Wrong username or password.'
            : 'Could not sign in. Is Identity.API running?';
      },
    });
  }
}
