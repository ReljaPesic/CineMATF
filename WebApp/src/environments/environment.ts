// List of backend endpoints for all services
// Default (production) environment.
export const environment = {
  production: true,
  api: {
    movies: 'http://localhost:8001/api/v1',
    cinema: 'http://localhost:8000/api/v1',
    screening: 'http://localhost:8003/api/v1',
    reservation: 'http://localhost:8002/api/v1',
    identity: 'http://localhost:8005/api/v1'
  },
};
