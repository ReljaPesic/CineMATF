import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Screening, ScreeningFilter, ScreeningRequest } from '../models/screening.model';

@Injectable({ providedIn: 'root' })
export class ScreeningService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.api.screening}/screening`;

  /** GET /screening?movieId=&date=&cinemaId= -> Screening[] (all filters optional, combinable) */
  getScreenings(filter: ScreeningFilter = {}): Observable<Screening[]> {
    let params = new HttpParams();
    if (filter.movieId) params = params.set('movieId', filter.movieId);
    if (filter.date) params = params.set('date', filter.date);
    if (filter.cinemaId) params = params.set('cinemaId', filter.cinemaId);
    return this.http.get<Screening[]>(this.baseUrl, { params });
  }

  /** GET /screening/{id} */
  getScreening(id: string): Observable<Screening> {
    return this.http.get<Screening>(`${this.baseUrl}/${id}`);
  }

  /** POST /screening */
  createScreening(request: ScreeningRequest): Observable<Screening> {
    return this.http.post<Screening>(this.baseUrl, request);
  }

  /** PUT /screening/{id} */
  updateScreening(id: string, request: ScreeningRequest): Observable<Screening> {
    return this.http.put<Screening>(`${this.baseUrl}/${id}`, request);
  }

  /** DELETE /screening/{id} */
  deleteScreening(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
