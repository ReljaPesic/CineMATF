import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Genre, Movie, MovieRequest, PagedResponse } from '../models/movie.model';

// This component is responsible for making POST requests to server 
// Each endpoint in the Movie service has a method that comunicates with it

@Injectable({ providedIn: 'root' })
export class MovieService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.api.movies}/movie`;

  /** GET /movie?page=&pageSize=  -> paged list */
  getMovies(page = 1, pageSize = 10): Observable<PagedResponse<Movie>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);
    return this.http.get<PagedResponse<Movie>>(this.baseUrl, { params });
  }

  /** GET /movie/{id} */
  getMovie(id: string): Observable<Movie> {
    return this.http.get<Movie>(`${this.baseUrl}/${id}`);
  }

  /** GET /movie/genre/{genre} */
  getByGenre(genre: Genre): Observable<Movie[]> {
    return this.http.get<Movie[]>(`${this.baseUrl}/genre/${genre}`);
  }

  /** GET /movie/search?title= */
  search(title: string): Observable<Movie[]> {
    const params = new HttpParams().set('title', title);
    return this.http.get<Movie[]>(`${this.baseUrl}/search`, { params });
  }

  /** POST /movie */
  createMovie(movie: MovieRequest): Observable<Movie> {
    return this.http.post<Movie>(this.baseUrl, movie);
  }

  /** PUT /movie/{id} */
  updateMovie(id: string, movie: MovieRequest): Observable<Movie> {
    return this.http.put<Movie>(`${this.baseUrl}/${id}`, movie);
  }

  /** DELETE /movie/{id} */
  deleteMovie(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
