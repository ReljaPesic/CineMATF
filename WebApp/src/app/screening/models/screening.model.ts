// Rewritten entities from Screening service into TS.
export enum ScreeningFormat {
  TwoD = 'TwoD',
  ThreeD = 'ThreeD',
  IMAX = 'IMAX',
}

export const ALL_FORMATS: ScreeningFormat[] = Object.values(ScreeningFormat);

export const SCREENING_FORMAT_LABELS: Record<ScreeningFormat, string> = {
  [ScreeningFormat.TwoD]: '2D',
  [ScreeningFormat.ThreeD]: '3D',
  [ScreeningFormat.IMAX]: 'IMAX',
};

export interface Screening {
  id: string;
  movieId: string;
  hallId: string;
  cinemaId: string;
  startTime: string;
  format: ScreeningFormat;
}

export type ScreeningRequest = Omit<Screening, 'id'>;

export interface ScreeningFilter {
  movieId?: string;
  date?: string;
  cinemaId?: string;
}
