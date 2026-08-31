// List of backend endpoints for all services
// Used during `ng serve`
export const environment = {
  production: false,
  api: {
    movies: 'http://localhost:8001/api/v1',
    cinema: 'http://localhost:8000/api/v1',
    screening: 'http://localhost:8003/api/v1'
  }
};
