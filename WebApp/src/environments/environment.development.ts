// List of backend endpoints for all services
// Used during `ng serve`
export const environment = {
  production: false,
  api: {
    movies: 'http://localhost:5011/api/v1',
    cinema: 'http://localhost:5000/api/v1',
    screening: 'http://localhost:8080/api/v1'
  }
};
