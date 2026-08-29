// Unit tests for MovieService 
// Start them by running `npm test`

import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { Genre, Movie, MovieRequest, PagedResponse } from '../models/movie.model';
import { MovieService } from './movie.service';
import { MOCK_MOVIE, MOCK_MOVIE_REQUEST, MOCK_MOVIES, MOCK_MOVIES_GENRE } from '../../../testing/mock_movies_data';


describe('MovieService', () => {
  const baseUrl = `${environment.api.movies}/movie`;

  let service: MovieService;
  let httpMock: HttpTestingController;

  const MOVIE = MOCK_MOVIE
  const MOVIES = MOCK_MOVIES
  const REQUEST = MOCK_MOVIE_REQUEST
  const MOVIES_BY_GENRE = MOCK_MOVIES_GENRE

  // Called before each unit test
  beforeEach(() => {
    // Injecting the needed services using TestBed
    TestBed.configureTestingModule({
      providers: [
        MovieService,
        provideHttpClient(), // real HttpClient...
        provideHttpClientTesting(), // ...backed by the fake backend
      ],
    });
    service = TestBed.inject(MovieService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  // Called after every unit test 
  afterEach(() => {
    httpMock.verify();
  });

  it('requests the paged list of movies', () => {
    const response: PagedResponse<Movie> = {
      data: MOVIES,
      page: 1,
      pageSize: 10,
      totalCount: 1,
    };

    let result: PagedResponse<Movie> | undefined;
    service.getMovies().subscribe((r) => (result = r));

    // Request should me made from one exact path 

    const req = httpMock.expectOne(`${baseUrl}?page=${response.page}&pageSize=${response.pageSize}`);

    // The service method should be GET
    expect(req.request.method).toBe('GET')
    // The service should default to page 1, pageSize 10.
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('10');

    // Trigger subscribe method from above with fake response
    req.flush(response);
    // Check if result is as expected
    expect(result).toEqual(response);
  });

  it('requests a single movie by id', () => {
    let result: Movie | undefined;
    service.getMovie(MOVIE.id).subscribe((m) => (result = m));

    const req = httpMock.expectOne(`${baseUrl}/${MOVIE.id}`);
    expect(req.request.method).toBe('GET');

    // Trigger subscribe method from above with fake response
    req.flush(MOVIE);
    // Check if result is as expected
    expect(result).toEqual(MOVIE);
  });

  it('requests movies for a specific genre', () => {
    let result: Movie[] | undefined;
    service.getByGenre(Genre.Crime).subscribe((m) => (result = m));

    const req = httpMock.expectOne(`${baseUrl}/genre/Crime`);
    expect(req.request.method).toBe('GET');

    req.flush(MOVIES_BY_GENRE);
    // Check if result is as expected
    expect(result).toEqual(MOVIES_BY_GENRE);
  });

  it('requests movie by title', () => {
    const title = 'The Godfather'
    let result: Movie[] | undefined;
    service.search(title).subscribe((m) => (result = m));

    const encodedTitle = encodeURIComponent(title)
    const req = httpMock.expectOne(`${baseUrl}/search?title=${encodedTitle}`);
    expect(req.request.method).toBe('GET')
    expect(req.request.params.get('title')).toBe(title);

    req.flush([MOVIE]);
    // Check if result is as expected
    expect(result).toEqual([MOVIE]);
  });


  it('create a movie using POST method', () => {
    let result: Movie | undefined;
    service.createMovie(REQUEST).subscribe((m) => (result = m));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(REQUEST);

    req.flush(MOVIE);
    // Check if result is as expected
    expect(result).toEqual(MOVIE);
  });

  it('updates movie using PUT method', () => {
    let result: Movie | undefined;
    service
      .updateMovie(MOVIE.id, REQUEST)
      .subscribe((m) => (result = m));

    const req = httpMock.expectOne(`${baseUrl}/${MOVIE.id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(REQUEST);

    req.flush(MOVIE);
    // Check if result is as expected
    expect(result).toEqual(MOVIE);
  });

  it('delete movie by id', () => {
    let completed = false;
    service.deleteMovie(MOVIE.id).subscribe(() => (completed = true));

    const req = httpMock.expectOne(`${baseUrl}/${MOVIE.id}`);
    expect(req.request.method).toBe('DELETE');

    req.flush(null); // body should be empty
    expect(completed).toBeTrue();
  });

  })