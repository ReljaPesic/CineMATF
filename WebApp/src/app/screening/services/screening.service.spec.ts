import { TestBed } from '@angular/core/testing';

import { ScreeningService } from './screening.service';

describe('ScreeningService', () => {
  let service: ScreeningService;

  // TODO: add tests for Screening services

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ScreeningService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
