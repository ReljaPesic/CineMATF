import { Genre, Movie, MovieRequest } from "../app/movies/models/movie.model";

export const MOCK_MOVIES : Movie[]= [
    {
    id: '1',
    title: "Inception",
    description: "A thief who steals corporate secrets through dream-sharing technology.",
    durationMinutes: 148,
    releaseDate: "2010-07-15",
    rating: 8.8,
    actors: [
    {
        firstName: "Leonardo",
        lastName: "DiCaprio"
    },
    {
        firstName: "Joseph",
        lastName: "Gordon-Levitt"
    }
    ],
    genres: [Genre.SciFi, Genre.Thriller, Genre.Action],
    coverImage: "https://resizing.flixster.com/9pwhgvMPsrGnmEtgThnX0rVzgts=/164x246/v2/https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p7825626_p_v10_ae.jpg"
},
{
    id: '2',
    title: "The Godfather",
    description: "The aging patriarch of an organized crime dynasty transfers control to his son.",
    durationMinutes: 175,
    releaseDate: "1972-03-23",
    rating: 9.2,
    actors: [
    {
        firstName: "Marlon",
        lastName: "Brando"
    },
    {
        firstName: "Al",
        lastName: "Pacino"
    }
    ],
    genres: [Genre.Crime, Genre.Drama],
    coverImage: "https://resizing.flixster.com/bUcMNsOCtzx1tV8Vp6HrpWA_Vo0=/164x246/v2/https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p6326_p_v12_be.jpg"
    },
    {
    id: '3',
    title: "The Dark Knight",
    description: "Batman faces the Joker, a criminal mastermind who plunges Gotham into anarchy.",
    durationMinutes: 152,
    releaseDate: "2008-07-17",
    rating: 9,
    actors: [
        {
        firstName: "Christian",
        lastName: "Bale"
        },
        {
        firstName: "Heath",
        lastName: "Ledger"
        }
    ],
    genres: [Genre.Action, Genre.Crime, Genre.Drama],
    coverImage: "https://resizing.flixster.com/PD15wDF15nqGushJNAW7av9-Tyk=/164x246/v2/https://resizing.flixster.com/Wg25mLoPWxjcxXzNyaSF4VGgGE4=/ems.cHJkLWVtcy1hc3NldHMvbW92aWVzL2ZiNjZiNWFkLWVhNzEtNDRhMC1iNGIwLTFmY2FkNzllNTJlMi5qcGc="
    }
]


export const MOCK_MOVIE: Movie = 
{
    id: '2',
    title: "The Godfather",
    description: "The aging patriarch of an organized crime dynasty transfers control to his son.",
    durationMinutes: 175,
    releaseDate: "1972-03-23",
    rating: 9.2,
    actors: [
    {
        firstName: "Marlon",
        lastName: "Brando"
    },
    {
        firstName: "Al",
        lastName: "Pacino"
    }
    ],
    genres: [Genre.Crime, Genre.Drama],
    coverImage: "https://resizing.flixster.com/bUcMNsOCtzx1tV8Vp6HrpWA_Vo0=/164x246/v2/https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p6326_p_v12_be.jpg"

}
export const MOCK_MOVIES_GENRE: Movie[] = [
    {
    id: '2',
    title: "The Godfather",
    description: "The aging patriarch of an organized crime dynasty transfers control to his son.",
    durationMinutes: 175,
    releaseDate: "1972-03-23",
    rating: 9.2,
    actors: [
    {
        firstName: "Marlon",
        lastName: "Brando"
    },
    {
        firstName: "Al",
        lastName: "Pacino"
    }
    ],
    genres: [Genre.Crime, Genre.Drama],
    coverImage: "https://resizing.flixster.com/bUcMNsOCtzx1tV8Vp6HrpWA_Vo0=/164x246/v2/https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p6326_p_v12_be.jpg"
    },
    {
    id: '3',
    title: "The Dark Knight",
    description: "Batman faces the Joker, a criminal mastermind who plunges Gotham into anarchy.",
    durationMinutes: 152,
    releaseDate: "2008-07-17",
    rating: 9,
    actors: [
        {
        firstName: "Christian",
        lastName: "Bale"
        },
        {
        firstName: "Heath",
        lastName: "Ledger"
        }
    ],
    genres: [Genre.Action, Genre.Crime, Genre.Drama],
    coverImage: "https://resizing.flixster.com/PD15wDF15nqGushJNAW7av9-Tyk=/164x246/v2/https://resizing.flixster.com/Wg25mLoPWxjcxXzNyaSF4VGgGE4=/ems.cHJkLWVtcy1hc3NldHMvbW92aWVzL2ZiNjZiNWFkLWVhNzEtNDRhMC1iNGIwLTFmY2FkNzllNTJlMi5qcGc="
    }
]
export const MOCK_MOVIE_REQUEST: MovieRequest = {
  title: MOCK_MOVIE.title,
  description: MOCK_MOVIE.description,
  durationMinutes: MOCK_MOVIE.durationMinutes,
  releaseDate: MOCK_MOVIE.releaseDate,
  rating: MOCK_MOVIE.rating,
  actors: MOCK_MOVIE.actors,
  genres: MOCK_MOVIE.genres,
  coverImage: MOCK_MOVIE.coverImage,
};
