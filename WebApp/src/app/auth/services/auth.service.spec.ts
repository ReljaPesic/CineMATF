import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { LocalStorageKeys } from '../../shared/local_storage/local_storage_keys';
import { AuthResponse } from '../models/auth.model';
import { MOCK_LOGIN_REQUEST, MOCK_REGISTER_REQUEST } from '../../../testing/mock_users_data';
import { jwtForUser } from '../../../testing/fake_jwt';

describe('AuthService', () => {
  const baseUrl = `${environment.api.identity}/Auth`;

  let httpMock: HttpTestingController;

  const LOGIN = MOCK_LOGIN_REQUEST;
  const REGISTER = MOCK_REGISTER_REQUEST;

  function makeService(): AuthService {
    return TestBed.inject(AuthService);
  }

  function authResponse(overrides: Partial<AuthResponse> = {}): AuthResponse {
    return { accessToken: jwtForUser(), refreshToken: 'refresh-token-1', ...overrides };
  }

  beforeEach(() => {
    //localStorage is shared across the whole Karma run, so it should be cleaned before each test
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(), 
        provideHttpClientTesting(), 
      ],
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('registers a new user', () => {
    const service = makeService();
    let created = false;
    service.register(REGISTER).subscribe(() => (created = true));

    const req = httpMock.expectOne(`${baseUrl}/RegisterUser`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(REGISTER);

    req.flush(null);
    expect(created).toBeTrue();
  });

  // when the service is created no user is logged in
  it('checks initial state - signed out', () => {
    const service = makeService();
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.user()).toBeNull();
    expect(service.isAdmin()).toBeFalse();
  });


  it('ignores an expired token in storage', () => {
    localStorage.setItem(
      LocalStorageKeys.AccessToken,
      JSON.stringify(jwtForUser({ expiresInSeconds: -60 })),
    );

    const service = makeService();
    expect(service.isLoggedIn()).toBeFalse();
    expect(service.user()).toBeNull();
  });

  it('logs in, stores the session and decodes the user', () => {
    const service = makeService();
    const result = authResponse({
      accessToken: jwtForUser({ sub: 'u1', name: 'alice', email: 'alice@cinematf.local', cardNumber: '4111111111111111' }),
    });

    let emitted: AuthResponse | undefined;
    service.login(LOGIN).subscribe((r) => (emitted = r));

    const req = httpMock.expectOne(`${baseUrl}/Login`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(LOGIN);
    req.flush(result);

    expect(emitted).toEqual(result);
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.user()?.username).toBe('alice');
    expect(service.user()?.cardNumber).toBe('4111111111111111');
    expect(service.token).toBe(result.accessToken);
    // checks if decoded data, placed in localStorage is as expected
    expect(JSON.parse(localStorage.getItem(LocalStorageKeys.RefreshToken)!)).toBe(result.refreshToken);
    expect(JSON.parse(localStorage.getItem(LocalStorageKeys.Username)!)).toBe('alice');
  });


  it('reports isAdmin when the token carries the Admin role', () => {
    const service = makeService();

    service.login(LOGIN).subscribe();
    const req = httpMock.expectOne(`${baseUrl}/Login`);
    req.flush(authResponse({ accessToken: jwtForUser({ roles: ['User', 'Admin'] }) }));
    expect(service.isAdmin()).toBeTrue();
  });

  it("doesn't report isAdmin when the token carries User role", () => {
    const service = makeService();

    service.login(LOGIN).subscribe();
    const req = httpMock.expectOne(`${baseUrl}/Login`);

    req.flush(authResponse({ accessToken: jwtForUser({ roles: ['User'] }) }));
    expect(service.isAdmin()).toBeFalse();
  })

  it('logs out and clears storage and the current user', () => {
    const service = makeService();
    service.login(LOGIN).subscribe();
    const req = httpMock.expectOne(`${baseUrl}/Login`);
    req.flush(authResponse());
    expect(service.isLoggedIn()).toBeTrue();

    service.logout();

    expect(service.isLoggedIn()).toBeFalse();
    expect(service.user()).toBeNull();
    expect(localStorage.getItem(LocalStorageKeys.AccessToken)).toBeNull();
    expect(localStorage.getItem(LocalStorageKeys.RefreshToken)).toBeNull();
    expect(localStorage.getItem(LocalStorageKeys.Username)).toBeNull();
  });

  // Guard clause: with no username + refresh token in storage there is nothing
  // to refresh, so the service skips the HTTP call, logs out, and emits null
  it('without stored credentials it logs out and emits null without calling the API', () => {
    const service = makeService();

    let emitted: string | null | undefined;
    service.refreshToken().subscribe((t) => (emitted = t));

    httpMock.expectNone(`${baseUrl}/Refresh`);
    expect(emitted).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
  });

  // when there is a user logged in and the refreshToken is stored we can refresh accessToken and refreshToken
  it('swaps stored credentials for a fresh pair and emits the new access token', () => {
    localStorage.setItem(LocalStorageKeys.Username, JSON.stringify('alice'));
    localStorage.setItem(LocalStorageKeys.RefreshToken, JSON.stringify('old-refresh'));
    const service = makeService();

    const fresh = authResponse({ accessToken: jwtForUser({ name: 'alice' }), refreshToken: 'new-refresh' });
    let emitted: string | null | undefined;
    service.refreshToken().subscribe((t) => (emitted = t));

    const req = httpMock.expectOne(`${baseUrl}/Refresh`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ userName: 'alice', refreshToken: 'old-refresh' });
    req.flush(fresh);

    expect(emitted).toBe(fresh.accessToken);
    expect(service.token).toBe(fresh.accessToken);
    expect(JSON.parse(localStorage.getItem(LocalStorageKeys.RefreshToken)!)).toBe('new-refresh');
  });


  it('fails refresh of tokens and logs out', () => {
    localStorage.setItem(LocalStorageKeys.Username, JSON.stringify('alice'));
    localStorage.setItem(LocalStorageKeys.RefreshToken, JSON.stringify('old-refresh'));
    const service = makeService();

    let emitted: string | null | undefined;
    service.refreshToken().subscribe((t) => (emitted = t));

    const req = httpMock.expectOne(`${baseUrl}/Refresh`);
    req.flush('nope', { status: 401, statusText: 'Unauthorized' });

    expect(emitted).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
    expect(localStorage.getItem(LocalStorageKeys.AccessToken)).toBeNull();
  });
});
