import { TestBed } from '@angular/core/testing';

import { UserService } from './user.service';
import { environment } from '../../../environments/environment';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { MOCK_USER, MOCK_USERNAME, MOCK_UPDATE_USER_REQUEST } from '../../../testing/mock_users_data';
import { UserDetails } from '../models/user.model';

describe('UserService', () => {
  // TODO: shange endpoint to lowercase
  const baseUrl = `${environment.api.identity}/User`;

  let service: UserService;
  let httpMock: HttpTestingController;

  const USER = MOCK_USER;
  const REQUEST = MOCK_UPDATE_USER_REQUEST;
  const USERNAME = MOCK_USERNAME;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        UserService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('requests user by username', () => {
    let result: UserDetails | undefined;
    service.getUser(USERNAME).subscribe((u) => (result = u));

    const req = httpMock.expectOne(`${baseUrl}/${USERNAME}`);
    expect(req.request.method).toBe('GET');

    req.flush(USER);
    expect(result).toEqual(USER);
  });

  it('updates user by username', () => {
    let result: UserDetails | undefined;
    service.updateUser(USERNAME, REQUEST).subscribe((u) => (result = u));

    const req = httpMock.expectOne(`${baseUrl}/${USERNAME}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(REQUEST);

    req.flush(USER);
    expect(result).toEqual(USER);
  });

  it('surfaces a 404 when the user does not exist', () => {
    let error: HttpErrorResponse | undefined;
    service.getUser('nobody').subscribe({
      next: () => fail('expected the request to error'),
      error: (err: HttpErrorResponse) => (error = err),
    });

    const req = httpMock.expectOne(`${baseUrl}/nobody`);
    req.flush('Not found', { status: 404, statusText: 'Not Found' });

    expect(error?.status).toBe(404);
  });
});
