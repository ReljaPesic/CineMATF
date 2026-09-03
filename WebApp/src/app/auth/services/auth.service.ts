import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, map, of, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LocalStorageService } from '../../shared/local_storage/local-storage.service';
import { LocalStorageKeys } from '../../shared/local_storage/local_storage_keys';
import { AuthResponse, CurrentUser, LoginRequest, RegisterRequest } from '../models/auth.model';

// Claim keys that Identity.API's TokenService writes into the JWT.
const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
const NAMEID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
const CARD_CLAIM = 'cardNumber';

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

  // Creates a normal (role "User") account. Returns 201 with no body on success.
  register(request: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/RegisterUser`, request);
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/Login`, request)
      .pipe(tap((res) => this.storeSession(res)));
  }

  // Swaps the stored refresh token for a fresh access/refresh pair.
  // Emits the new access token, or null when refreshing isn't possible (and
  // then logs out). Used by the HTTP interceptor to recover from a 401.
  refreshToken(): Observable<string | null> {
    const userName = this.storage.get(LocalStorageKeys.Username) as string | null;
    const refreshToken = this.storage.get(LocalStorageKeys.RefreshToken) as string | null;

    if (!userName || !refreshToken) {
      this.logout();
      return of(null);
    }

    return this.http
      .post<AuthResponse>(`${this.baseUrl}/Refresh`, { userName, refreshToken })
      .pipe(
        tap((res) => this.storeSession(res)),
        map((res) => res.accessToken),
        catchError(() => {
          this.logout();
          return of(null);
        }),
      );
  }

  logout(): void {
    this.storage.clear(LocalStorageKeys.AccessToken);
    this.storage.clear(LocalStorageKeys.RefreshToken);
    this.storage.clear(LocalStorageKeys.Username);
    this.currentUser.set(null);
  }

  private storeSession(res: AuthResponse): void {
    const user = this.decode(res.accessToken);
    this.storage.set(LocalStorageKeys.AccessToken, res.accessToken);
    this.storage.set(LocalStorageKeys.RefreshToken, res.refreshToken);
    if (user) {
      this.storage.set(LocalStorageKeys.Username, user.username);
    }
    this.currentUser.set(user);
  }

  // Reads the JWT payload without verifying the signature 
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
        id: payload.sub ?? payload[NAMEID_CLAIM] ?? '',
        username: payload[NAME_CLAIM] ?? payload.sub ?? '',
        email: payload.email ?? payload[EMAIL_CLAIM] ?? null,
        roles: Array.isArray(rawRoles) ? rawRoles : [rawRoles],
        cardNumber: payload[CARD_CLAIM] || null,
      };
    } catch {
      return null;
    }
  }
}