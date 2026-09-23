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
  imdbUrl?: string | null;
}

export interface OmdbMovieDetails {
  Title: string;
  Year: string;
  Rated: string;
  Released: string;
  Runtime: string;
  Genre: string;
  Director: string;
  Writer: string;
  Actors: string;
  Plot: string;
  Language: string;
  Country: string;
  Awards: string;
  Poster: string;
  Ratings: { Source: string; Value: string }[];
  Metascore: string;
  imdbRating: string;
  imdbVotes: string;
  imdbID: string;
  Type: string;
  Response: string;
  Error?: string;
}

export type MovieRequest = Omit<Movie, 'id'>;
