import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { PagedResponse } from '../../shared/models/shared.models';
import { Cinema, CinemaRequest, City, CreateHallsRequest, CreateHallsResponse, HallResponse, SeatResponse, UpdateSeatTypeRequest } from '../models/cinema.model';


@Injectable({providedIn: 'root'})
export class CinemaService {

  private readonly http = inject(HttpClient)
  private readonly baseUrl = `${environment.api.cinema}/cinema`;

  /* GET /cinema?page=1&pageSize=10 -> PagedResponse<CinemaResponse>`*/
  getCinemas(page = 1, pageSize = 10): Observable<PagedResponse<Cinema>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize)
    return this.http.get<PagedResponse<Cinema>>(this.baseUrl, {params});
  }

  /* GET /cinema/city/{cityName} -> Cinema[] */
  getCinemaByCity(city: City): Observable<Cinema[]>{
    return this.http.get<Cinema[]>(`${this.baseUrl}/city/${city}`)
  }

  /* GET /cinema/{id:guid} -> CinemaResponse*/
  getCinema(id: string): Observable<Cinema>{
    return this.http.get<Cinema>(`${this.baseUrl}/${id}`);
  }

  /* POST /cinema */
  createCinema(cinema: CinemaRequest): Observable<Cinema>{
    return this.http.post<Cinema>(this.baseUrl, cinema)
  }

  /* PUT /cinema/{id:guid} */
  updateCinema(cinemaId: string, cinema: CinemaRequest): Observable<Cinema>{
    return this.http.put<Cinema>(`${this.baseUrl}/${cinemaId}`, cinema);
  }
  /* DELETE /cinema/{id:guid} */
  deleteCinema(id: string): Observable<void>{
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  /* GET /cinema/{cinemaId}/halls -> HallResponse[]*/
  getHallsByCinemaId(id: string): Observable<HallResponse[]>{
    return this.http.get<HallResponse[]>(`${this.baseUrl}/${id}/halls`);
  }

  /* POST /cinema/{cinemaId}/halls */
  createHalls(cinemaId: string, halls: CreateHallsRequest): Observable<CreateHallsResponse>{
    return this.http.post<CreateHallsResponse>(`${this.baseUrl}/${cinemaId}/halls`, halls)
  }

  /* DELETE /cinema/{cinemaId}/halls/{hallId}  */
  deleteHallByCinemaAndHallId(cinemaId: string, hallId: string): Observable<void>{
    return this.http.delete<void>(`${this.baseUrl}/${cinemaId}/halls/${hallId}`)
  }

  /* GET /cinema/{cinemaId}/halls/{hallId}/seats -> SeatResponse[] */
  getSeatsByCinemaAndHallIds(cinemaId: string, hallId: string): Observable<SeatResponse[]>{
    return this.http.get<SeatResponse[]>(`${this.baseUrl}/${cinemaId}/halls/${hallId}/seats`);
  }

  /* POST /cinema/{cinemaId}/halls/{hallId}/seats */
  createSeats(cinemaId: string, hallId: string): Observable<void>{
    return this.http.post<void>(`${this.baseUrl}/${cinemaId}/halls/${hallId}/seats`, null);
  }

  /* PaTCH /cinema/{cinemaId}/halls/{hallId}/seats/{seatId} */
  updateSeatType(cinemaId: string, hallId: string, seatId: string, seat: UpdateSeatTypeRequest): Observable<SeatResponse>{
    return this.http.patch<SeatResponse>(`${this.baseUrl}/${cinemaId}/halls/${hallId}/seats/${seatId}`, seat);
  }

  /* GET /cinema/seats/{seatId} -> SeatResponse*/
  getSeat(id: string): Observable<SeatResponse>{
    return this.http.get<SeatResponse>(`${this.baseUrl}/seats/${id}`);
  }
}
