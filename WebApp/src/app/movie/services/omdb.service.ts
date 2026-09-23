import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { OmdbMovieDetails } from '../models/movie.model';

import { catchError, map, Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OmdbService {
  private readonly http = inject(HttpClient)
  private readonly apiKey = 'da53126b';
  private readonly baseUrl = 'http://www.omdbapi.com/';

  getMovieByTitle(title: string): Observable<OmdbMovieDetails | null>{
    const url = `${this.baseUrl}?t=${encodeURIComponent(title)}&apikey=${this.apiKey}`;

    return this.http.get<OmdbMovieDetails>(url).pipe(
      map((res) => res.Response === 'False' ? null : res),
      catchError(() => of(null))
    )
  }
}
