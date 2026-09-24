import { Component, OnInit, inject } from '@angular/core';
import { forkJoin } from 'rxjs';

import { Cinema } from '../../models/cinema.model';
import { CinemaService } from '../../services/cinema.service';
import { UserDetails } from '../../../user/models/user.model';
import { UserService } from '../../../user/services/user.service';

@Component({
  selector: 'app-cinema-admin-list',
  standalone: false,
  templateUrl: './cinema-admin-list.component.html',
  styleUrl: './cinema-admin-list.component.css',
})
export class CinemaAdminListComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly cinemaService = inject(CinemaService);

  cinemaAdmins: UserDetails[] = [];
  private cinemaNames = new Map<string, string>();

  loading = false;
  error: string | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;

    forkJoin({
      users: this.userService.getAllUsers(),
      cinemas: this.cinemaService.getCinemas(1, 100),
    }).subscribe({
      next: ({ users, cinemas }) => {
        this.cinemaNames = new Map(cinemas.data.map((c: Cinema) => [c.id, c.name]));
        this.cinemaAdmins = users.filter((u) => u.roles.includes('CinemaAdmin'));
        this.loading = false;
      },
      error: (err) => {
        console.error('Could not load cinema admins', err);
        this.error = 'Could not load cinema admins.';
        this.loading = false;
      },
    });
  }

  cinemaNamesFor(user: UserDetails): string {
    return user.cinemaIds.map((id) => this.cinemaNames.get(id) ?? '(unknown cinema)').join(', ');
  }
}
