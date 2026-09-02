import { TestBed } from '@angular/core/testing';

import { CinemaService } from './cinema.service';
import { environment } from '../../../environments/environment';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PagedResponse } from '../../shared/models/shared.models';
import { Cinema, HallResponse, SeatResponse } from '../models/cinema.model';
import {
  MOCK_CINEMA,
  MOCK_CINEMAS,
  MOCK_CINEMAS_BY_CITY,
  MOCK_CINEMA_REQUEST,
  MOCK_HALL,
  MOCK_HALLS,
  MOCK_SEAT,
  MOCK_SEATS,
  MOCK_UPDATE_SEAT_TYPE_REQUEST,
} from '../../../testing/mock_cinemas_data';

describe('CinemaService', () => {
  const baseUrl= `${environment.api.cinema}/cinema`;
  let service: CinemaService;
  let httpMock: HttpTestingController;

  const CINEMA = MOCK_CINEMA;
  const CINEMAS = MOCK_CINEMAS;
  const REQUEST = MOCK_CINEMA_REQUEST;
  const CINEMAS_BY_CITY = MOCK_CINEMAS_BY_CITY;
  const HALL = MOCK_HALL;
  const HALLS = MOCK_HALLS;
  const SEAT = MOCK_SEAT;
  const SEATS = MOCK_SEATS;
  const UPDATE_SEAT_TYPE_REQUEST = MOCK_UPDATE_SEAT_TYPE_REQUEST;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        CinemaService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ]
    });
    service = TestBed.inject(CinemaService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterAll(() => {
    httpMock.verify();
  })

  it('requests the paged list of cinemas', () => {
    const response: PagedResponse<Cinema> = {
      data: CINEMAS,
      page: 1,
      pageSize: 10,
      totalCount: 1
    }
    let result: PagedResponse<Cinema> | undefined;
    service.getCinemas().subscribe((r) => {result = r});

    const req = httpMock.expectOne(`${baseUrl}?page=${response.page}&pageSize=${response.pageSize}`);

    expect(req.request.method).toBe('GET');
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('10');

    req.flush(response);
    expect(result).toEqual(response);
  })

  it('requests cinemas by city', () => {
      let result: Cinema[] | undefined;
      service.getCinemaByCity(CINEMA.city).subscribe((c) => result = c)

      const req = httpMock.expectOne(`${baseUrl}/city/${CINEMA.city}`);
      expect(req.request.method).toBe('GET')
  
      req.flush(CINEMAS_BY_CITY);
      expect(result).toEqual(CINEMAS_BY_CITY);
    });

  it('requests cinema by id', () => {
      let result: Cinema | undefined;
      service.getCinema(CINEMA.id).subscribe((c) => result = c)

      const req = httpMock.expectOne(`${baseUrl}/${CINEMA.id}`);
      expect(req.request.method).toBe('GET')
  
      req.flush(CINEMA);
      expect(result).toEqual(CINEMA);
    });
  
  
    it('create a cinema using POST method', () => {
      let result: Cinema | undefined;
      service.createCinema(REQUEST).subscribe((m) => (result = m));
  
      const req = httpMock.expectOne(baseUrl);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(REQUEST);
  
      req.flush(CINEMA);
      expect(result).toEqual(CINEMA);
    });
  
    it('updates cinema using PUT method', () => {
      let result: Cinema | undefined;
      service
        .updateCinema(CINEMA.id, REQUEST)
        .subscribe((m) => (result = m));
  
      const req = httpMock.expectOne(`${baseUrl}/${CINEMA.id}`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(REQUEST);
  
      req.flush(CINEMA);
      expect(result).toEqual(CINEMA);
    });
  
    it('delete cinema by id', () => {
      let completed = false;
      service.deleteCinema(CINEMA.id).subscribe(() => (completed = true));
  
      const req = httpMock.expectOne(`${baseUrl}/${CINEMA.id}`);
      expect(req.request.method).toBe('DELETE');
  
      req.flush(null); 
      expect(completed).toBeTrue();
    });

    it('requests halls for cinema', () => {
      let result: HallResponse[] | undefined;
      service.getHallsByCinemaId(CINEMA.id).subscribe((h)=> result = h);

      const req = httpMock.expectOne(`${baseUrl}/${CINEMA.id}/halls`);
      expect(req.request.method).toBe('GET');

      req.flush(HALLS)
      expect(result).toEqual(HALLS)
    })

    it('delete hall by id and cinema id', () => {
      let completed = false;
      service.deleteHallByCinemaAndHallId(CINEMA.id, HALL.id).subscribe(() => completed = true);

      const req = httpMock.expectOne(`${baseUrl}/${CINEMA.id}/halls/${HALL.id}`);
      expect(req.request.method).toBe('DELETE');

      req.flush(null);
      expect(completed).toBeTrue();
    });

    it('requests seats by hall adn cinema ids', () => {
      let result : SeatResponse[] | undefined;
      service.getSeatsByCinemaAndHallIds(CINEMA.id, HALL.id).subscribe((r) => result = r);

      const req = httpMock.expectOne(`${baseUrl}/${CINEMA.id}/halls/${HALL.id}/seats`);
      expect(req.request.method).toBe('GET');

      req.flush(SEATS)
      expect(result).toEqual(SEATS);
    });

    it('creates seats for a hall', () => {
      let created = false;
      service.createSeats(CINEMA.id, HALL.id).subscribe(()=> created = true);

      const req = httpMock.expectOne(`${baseUrl}/${CINEMA.id}/halls/${HALL.id}/seats`);
      expect(req.request.method).toBe('POST');

      req.flush(null);
      expect(created).toBeTrue();
    });

    it('edit seat type for a hall in a cinema', () => {
      let updated = false;
      service
        .updateSeatType(CINEMA.id, HALL.id, SEAT.id, UPDATE_SEAT_TYPE_REQUEST)
        .subscribe(() => updated = true);

      const req = httpMock.expectOne(`${baseUrl}/${CINEMA.id}/halls/${HALL.id}/seats/${SEAT.id}`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.body).toEqual(UPDATE_SEAT_TYPE_REQUEST);

      req.flush(SEAT);
      expect(updated).toBeTrue();
    });

    it('requests seat by id', () => {
      let result: SeatResponse | undefined;
      service.getSeat(SEAT.id).subscribe((s) => result = s);

      const req = httpMock.expectOne(`${baseUrl}/seats/${SEAT.id}`);
      expect(req.request.method).toBe('GET');

      req.flush(SEAT);
      expect(result).toEqual(SEAT);
    })
});
