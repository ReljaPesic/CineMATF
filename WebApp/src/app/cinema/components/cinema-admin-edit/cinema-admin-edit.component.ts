import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';

import { Cinema } from '../../models/cinema.model';
import { CinemaService } from '../../services/cinema.service';
import { UserService } from '../../../user/services/user.service';

@Component({
  selector: 'app-cinema-admin-edit',
  standalone: false,
  templateUrl: './cinema-admin-edit.component.html',
  styleUrl: './cinema-admin-edit.component.css',
})
export class CinemaAdminEditComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly userService = inject(UserService);
  private readonly cinemaService = inject(CinemaService);

  username = '';
  cinemas: Cinema[] = [];
  readonly selectedCinemaIds = new Set<string>();

  loading = true;
  notFound = false;
  saving = false;
  error: string | null = null;

  ngOnInit(): void {
    this.username = this.route.snapshot.paramMap.get('username')!;

    forkJoin({
      user: this.userService.getUser(this.username),
      cinemas: this.cinemaService.getCinemas(1, 100),
    }).subscribe({
      next: ({ user, cinemas }) => {
        if (!user.roles.includes('CinemaAdmin')) {
          this.notFound = true;
          this.loading = false;
          return;
        }
        this.cinemas = cinemas.data;
        user.cinemaIds.forEach((id) => this.selectedCinemaIds.add(id));
        this.loading = false;
      },
      error: (err: HttpErrorResponse) => {
        this.loading = false;
        this.notFound = err.status === 404;
        if (!this.notFound) {
          this.error = 'Could not load this cinema admin.';
        }
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
    if (this.saving) return;

    if (this.selectedCinemaIds.size === 0) {
      this.error = 'Select at least one cinema.';
      return;
    }

    this.saving = true;
    this.error = null;

    this.userService.updateCinemaAssignment(this.username, { cinemaIds: [...this.selectedCinemaIds] }).subscribe({
      next: () => this.router.navigate(['/cinemas/admins']),
      error: (err: HttpErrorResponse) => {
        this.saving = false;
        console.error('Updating cinema assignment failed', err);
        this.error = 'Could not save changes. Check the fields and try again.';
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/cinemas/admins']);
  }
}
