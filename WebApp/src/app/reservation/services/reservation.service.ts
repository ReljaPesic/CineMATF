import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  AvailableSeats,
  CreateReservationRequest,
  Reservation,
  Ticket,
} from '../models/reservation.model';


@Injectable({ providedIn: 'root' })
export class ReservationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.api.reservation}/reservations`;
  private readonly ticketUrl = `${environment.api.reservation}/Ticket`;

  /** GET /reservations */
  getReservations(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(this.baseUrl);
  }

  /** GET /reservations/{id} */
  getReservation(id: string): Observable<Reservation> {
    return this.http.get<Reservation>(`${this.baseUrl}/${id}`);
  }

  /** GET /reservations/screenings/{screeningId}/available-seats */
  getAvailableSeats(screeningId: string): Observable<AvailableSeats> {
    return this.http.get<AvailableSeats>(
      `${this.baseUrl}/screenings/${screeningId}/available-seats`,
    );
  }

  /** POST /reservations -> creates a Locked reservation that holds the seats */
  createReservation(request: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(this.baseUrl, request);
  }

  /** POST /reservations/{id}/pay -> Locked -> Confirmed */
  pay(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/pay`, null);
  }

  /** POST /reservations/{id}/cancel -> Locked -> Cancelled, releases the seats */
  cancel(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/cancel`, null);
  }

  /** POST /Ticket/reservation/{reservationId} -> issues tickets for a confirmed reservation */
  generateTickets(reservationId: string): Observable<Ticket[]> {
    return this.http.post<Ticket[]>(`${this.ticketUrl}/reservation/${reservationId}`, null);
  }

  /** GET /Ticket/{id}/download -> plain-text ticket file (open directly in the browser) */
  ticketDownloadUrl(ticketId: string): string {
    return `${this.ticketUrl}/${ticketId}/download`;
  }
}
