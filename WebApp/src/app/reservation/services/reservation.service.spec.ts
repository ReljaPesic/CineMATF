import { TestBed } from '@angular/core/testing';

import { ReservationService } from './reservation.service';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { environment } from '../../../environments/environment';
import {
  MOCK_AVAILABLE_SEATS,
  MOCK_CREATE_RESERVATION_REQUEST,
  MOCK_RESERVATION,
  MOCK_RESERVATIONS,
  MOCK_TICKETS,
} from '../../../testing/mock_reservations_data';
import { AvailableSeats, Reservation, Ticket } from '../models/reservation.model';

describe('ReservationService', () => {
  const reservationsUrl = `${environment.api.reservation}/reservations`;
  const ticketUrl = `${environment.api.reservation}/Ticket`;

  let httpMock: HttpTestingController;
  let service: ReservationService;

  const RESERVATION = MOCK_RESERVATION;
  const RESERVATIONS = MOCK_RESERVATIONS;
  const REQUEST = MOCK_CREATE_RESERVATION_REQUEST;
  const AVAILABLE_SEATS = MOCK_AVAILABLE_SEATS;
  const TICKETS = MOCK_TICKETS;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ReservationService,
        provideHttpClient(), 
        provideHttpClientTesting(), 
      ],
    });
    service = TestBed.inject(ReservationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('requests every reservation', () => {
    let result: Reservation[] | undefined;
    service.getReservations().subscribe((r) => (result = r));

    const req = httpMock.expectOne(reservationsUrl);
    expect(req.request.method).toBe('GET');

    req.flush(RESERVATIONS);
    expect(result).toEqual(RESERVATIONS);
  });

  it('requests a single reservation by id', () => {
    let result: Reservation | undefined;
    service.getReservation(RESERVATION.id).subscribe((r) => (result = r));

    const req = httpMock.expectOne(`${reservationsUrl}/${RESERVATION.id}`);
    expect(req.request.method).toBe('GET');

    req.flush(RESERVATION);
    expect(result).toEqual(RESERVATION);
  });

  it('requests the available seats for a screening', () => {
    let result:AvailableSeats | undefined;
    service.getAvailableSeats(AVAILABLE_SEATS.screeningId).subscribe((r) => (result = r));

    const req = httpMock.expectOne(
      `${reservationsUrl}/screenings/${AVAILABLE_SEATS.screeningId}/available-seats`,
    );
    expect(req.request.method).toBe('GET');

    req.flush(AVAILABLE_SEATS);
    expect(result).toEqual(AVAILABLE_SEATS);
  });

  it('creates a reservation', () => {
    let result: Reservation | undefined;
    service.createReservation(REQUEST).subscribe((r) => (result = r));

    const req = httpMock.expectOne(reservationsUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(REQUEST);

    req.flush(RESERVATION);
    expect(result).toEqual(RESERVATION);
  });

  it('pays for a reservation', () => {
    let completed = false;
    service.pay(RESERVATION.id).subscribe(() => (completed = true));

    const req = httpMock.expectOne(`${reservationsUrl}/${RESERVATION.id}/pay`);
    expect(req.request.method).toBe('POST');

    req.flush(null);
    expect(completed).toBeTrue();
  });

  it('cancels a reservation', () => {
    let completed = false;
    service.cancel(RESERVATION.id).subscribe(() => (completed = true));

    const req = httpMock.expectOne(`${reservationsUrl}/${RESERVATION.id}/cancel`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeNull();

    req.flush(null);
    expect(completed).toBeTrue();
  });

  it('generates tickets for a reservation', () => {
    let result: Ticket[] | undefined;
    service.generateTickets(RESERVATION.id).subscribe((t) => (result = t));

    const req = httpMock.expectOne(`${ticketUrl}/reservation/${RESERVATION.id}`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toBeNull();

    req.flush(TICKETS);
    expect(result).toEqual(TICKETS);
  });

  it('builds a ticket download url without hitting the network', () => {
    const ticketId = TICKETS[0].id;
    expect(service.ticketDownloadUrl(ticketId)).toBe(`${ticketUrl}/${ticketId}/download`);
  });
});
