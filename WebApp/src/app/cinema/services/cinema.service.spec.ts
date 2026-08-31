import { TestBed } from '@angular/core/testing';

import { CinemaService } from './cinema.service';

describe('CinemaService', () => {
  let service: CinemaService;
  //TODO: add tests for cinema service

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(CinemaService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
