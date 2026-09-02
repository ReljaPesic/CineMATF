import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthService } from '../../../auth/services/auth.service';
import { UpdateUserRequest, UserDetails } from '../../models/user.model';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly userService = inject(UserService);

  readonly username = this.auth.user()?.username ?? '';
  readonly roles = this.auth.user()?.roles ?? [];

  form = { firstName: '', lastName: '', email: '', cardNumber: '', phoneNumber: '' };

  loading = true;
  saving = false;
  error: string | null = null;
  saved = false;

  ngOnInit(): void {
    if (!this.username) {
      this.loading = false;
      this.error = 'You are not signed in.';
      return;
    }

    this.userService.getUser(this.username).subscribe({
      next: (user) => {
        this.fill(user);
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.error =
          err.status === 403
            ? 'You are not allowed to view this profile.'
            : 'Could not load your profile.';
      },
    });
  }

  save(): void {
    if (this.saving) return;
    this.saving = true;
    this.error = null;
    this.saved = false;

    const request: UpdateUserRequest = {
      firstName: this.form.firstName.trim(),
      lastName: this.form.lastName.trim(),
      email: this.form.email.trim(),
      cardNumber: this.form.cardNumber.trim(),
      phoneNumber: this.form.phoneNumber.trim() || null,
    };

    this.userService.updateUser(this.username, request).subscribe({
      next: (user) => {
        this.fill(user);
        this.saving = false;
        this.saved = true;
      },
      error: () => {
        this.saving = false;
        this.error = 'Could not save your changes.';
      },
    });
  }

  private fill(user: UserDetails): void {
    this.form = {
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      cardNumber: user.cardNumber,
      phoneNumber: user.phoneNumber ?? '',
    };
  }
}
