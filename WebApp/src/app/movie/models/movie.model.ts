// Rewritten entities from Movies service into TS

export enum Genre {
  Action = 'Action',
  Animation = 'Animation',
  Comedy = 'Comedy',
  Crime = 'Crime',
  Documentary = 'Documentary',
  Drama = 'Drama',
  Fantasy = 'Fantasy',
  Horror = 'Horror',
  Mystery = 'Mystery',
  Romance = 'Romance',
  SciFi = 'SciFi',
  Thriller = 'Thriller',
}

/** Every genre value, for filter dropdowns and the movie form's checkboxes. */
export const ALL_GENRES: Genre[] = Object.values(Genre);

export interface Actor {
  firstName: string;
  lastName: string;
}

export interface Movie {
  id: string;
  title: string;
  description: string;
  durationMinutes: number;
  releaseDate: string; 
  rating: number;
  actors: Actor[];
  genres: Genre[];
  coverImage?: string | null;
}


export type MovieRequest = Omit<Movie, 'id'>;
