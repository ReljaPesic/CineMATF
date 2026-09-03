import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { ScreeningService } from './screening.service';
import { environment } from '../../../environments/environment';
import { Screening } from '../models/screening.model';
import {
  MOCK_SCREENING,
  MOCK_SCREENINGS,
  MOCK_SCREENINGS_BY_MOVIE,
  MOCK_SCREENING_REQUEST,
} from '../../../testing/mock_screenings_data';

describe('ScreeningService', () => {
  const baseUrl = `${environment.api.screening}/screening`;

  let service: ScreeningService;
  let httpMock: HttpTestingController;

  const SCREENING = MOCK_SCREENING;
  const SCREENINGS = MOCK_SCREENINGS;
  const SCREENINGS_BY_MOVIE = MOCK_SCREENINGS_BY_MOVIE;
  const REQUEST = MOCK_SCREENING_REQUEST;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ScreeningService,
        provideHttpClient(),
        provideHttpClientTesting(),
      ],
    });
    service = TestBed.inject(ScreeningService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });


  it('requests every screening when no filter is given', () => {
    let result: Screening[] | undefined;
    service.getScreenings().subscribe((r) => (result = r));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    expect(req.request.params.keys().length).toBe(0);

    req.flush(SCREENINGS);
    expect(result).toEqual(SCREENINGS);
  });

  it('requests a single screening by id', () => {
    let result: Screening | undefined;
    service.getScreening(SCREENING.id).subscribe((r) => (result = r));

    const req = httpMock.expectOne(`${baseUrl}/${SCREENING.id}`);
    expect(req.request.method).toBe('GET');

    req.flush(SCREENING);
    expect(result).toEqual(SCREENING);
  });

  it('creates a screening', () => {
    let result: Screening | undefined;
    service.createScreening(REQUEST).subscribe((r) => (result = r));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(REQUEST);

    req.flush(SCREENING);
    expect(result).toEqual(SCREENING);
  });

  it('updates a screening', () => {
    let result: Screening | undefined;
    service.updateScreening(SCREENING.id, REQUEST).subscribe((r) => (result = r));

    const req = httpMock.expectOne(`${baseUrl}/${SCREENING.id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(REQUEST);

    req.flush(SCREENING);
    expect(result).toEqual(SCREENING);
  });

  it('deletes a screening by id', () => {
    let completed = false;
    service.deleteScreening(SCREENING.id).subscribe(() => (completed = true));

    const req = httpMock.expectOne(`${baseUrl}/${SCREENING.id}`);
    expect(req.request.method).toBe('DELETE');

    req.flush(null);
    expect(completed).toBeTrue();
  });
});
