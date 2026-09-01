import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LocalStorageService } from '../../shared/local_storage/local-storage.service';
import { LocalStorageKeys } from '../../shared/local_storage/local_storage_keys';
import { AuthResponse, CurrentUser, LoginRequest } from '../models/auth.model';

// Claim URIs that Identity.API's TokenService writes into the JWT
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
const NAMEID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storage = inject(LocalStorageService);
  private readonly baseUrl = `${environment.api.identity}/Auth`;

  private readonly currentUser = signal<CurrentUser | null>(this.decode(this.token));

  readonly user = this.currentUser.asReadonly();
  readonly isLoggedIn = computed(() => this.currentUser() !== null);
  readonly isAdmin = computed(() => this.currentUser()?.roles.includes('Admin') ?? false);

  get token(): string | null {
    return this.storage.get(LocalStorageKeys.AccessToken);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/Login`, request).pipe(
      tap((res) => {
        this.storage.set(LocalStorageKeys.AccessToken, res.accessToken);
        this.storage.set(LocalStorageKeys.RefreshToken, res.refreshToken);
        this.currentUser.set(this.decode(res.accessToken));
      }),
    );
  }

  logout(): void {
    this.storage.clear(LocalStorageKeys.AccessToken);
    this.storage.clear(LocalStorageKeys.RefreshToken);
    this.currentUser.set(null);
  }

  // Reads the JWT payload without verifying the signature - the APIs do that.
  // Returns null for a missing, malformed, or expired token.
  private decode(token: string | null): CurrentUser | null {
    if (!token) return null;
    try {
      const json = atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/'));
      const payload = JSON.parse(json);

      if (typeof payload.exp === 'number' && payload.exp * 1000 <= Date.now()) {
        return null;
      }

      const rawRoles = payload[ROLE_CLAIM] ?? [];
      return {
        // "sub" is the standard spot for the user id; nameidentifier is the
        // fallback. Identity.API must put one of them in the token, otherwise
        // there is no id to book reservations with.
        id: payload.sub ?? payload[NAMEID_CLAIM] ?? '',
        username: payload[NAME_CLAIM] ?? payload.sub ?? '',
        email: payload.email ?? payload[EMAIL_CLAIM] ?? null,
        roles: Array.isArray(rawRoles) ? rawRoles : [rawRoles],
      };
    } catch {
      return null;
    }
  }
}
