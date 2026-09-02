import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { UpdateUserRequest, UserDetails } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.api.identity}/User`;

  /** GET /api/v1/User/{username} */
  getUser(username: string): Observable<UserDetails> {
    return this.http.get<UserDetails>(`${this.baseUrl}/${username}`);
  }

  /** PUT /api/v1/User/{username} */
  updateUser(username: string, request: UpdateUserRequest): Observable<UserDetails> {
    return this.http.put<UserDetails>(`${this.baseUrl}/${username}`, request);
  }
}
