import { Screening, ScreeningFormat, ScreeningRequest } from '../app/screening/models/screening.model';

function futureIso(daysAhead: number, hour: number, minute = 0): string {
  const d = new Date();
  d.setDate(d.getDate() + daysAhead);
  d.setHours(hour, minute, 0, 0);
  return d.toISOString();
}

export const MOCK_SCREENINGS: Screening[] = [
  { id: 's1', movieId: '1', hallId: 'c1-h1', cinemaId: 'c1', startTime: futureIso(1, 18, 0), format: ScreeningFormat.TwoD },
  { id: 's2', movieId: '1', hallId: 'c1-h2', cinemaId: 'c1', startTime: futureIso(1, 21, 0), format: ScreeningFormat.IMAX },
  { id: 's3', movieId: '3', hallId: 'c2-h1', cinemaId: 'c2', startTime: futureIso(2, 19, 0), format: ScreeningFormat.TwoD },
  { id: 's4', movieId: '2', hallId: 'c2-h2', cinemaId: 'c2', startTime: futureIso(3, 20, 30), format: ScreeningFormat.ThreeD },
  { id: 's5', movieId: '3', hallId: 'c3-h1', cinemaId: 'c3', startTime: futureIso(5, 17, 30), format: ScreeningFormat.TwoD },
];

export const MOCK_SCREENING: Screening = MOCK_SCREENINGS[0];

export const MOCK_SCREENINGS_BY_MOVIE: Screening[] = MOCK_SCREENINGS.filter((s) => s.movieId === '1');

export const MOCK_SCREENING_REQUEST: ScreeningRequest = {
  movieId: '1',
  hallId: 'c1-h1',
  cinemaId: 'c1',
  startTime: futureIso(4, 19, 0),
  format: ScreeningFormat.TwoD,
};
