// Response of GET /api/v1/User/{username} and GET /api/v1/User (Identity.API).
export interface UserDetails {
  id: string;
  userName: string;
  firstName: string;
  lastName: string;
  email: string;
  cardNumber: string;
  phoneNumber: string | null;
  roles: string[];
  cinemaIds: string[];
}

// Body of PUT /api/v1/User/{username}. Username and password aren't editable here.
export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  cardNumber: string;
  phoneNumber: string | null;
}

// Body of PUT /api/v1/User/{username}/cinemas - SuperAdmin reassigning an
// existing CinemaAdmin's cinema(s).
export interface UpdateCinemaAssignmentRequest {
  cinemaIds: string[];
}
