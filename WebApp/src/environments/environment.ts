// List of backend endpoints for all services
// Default (production) environment.
export const environment = {
  production: true,
  api: {
    movies: 'http://localhost:5011/api/v1',
    cinema: 'http://localhost:5000/api/v1',
    screening: 'http://localhost:5138/api/v1'
  },
};
