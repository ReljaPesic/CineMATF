// Response of GET /api/v1/User/{username} (Identity.API).
export interface UserDetails {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  cardNumber: string;
  phoneNumber: string | null;
}

// Body of PUT /api/v1/User/{username}. Username and password aren't editable here.
export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  cardNumber: string;
  phoneNumber: string | null;
}
