// Body of POST /api/v1/Auth/Login (Identity.API).
export interface LoginRequest {
  userName: string;
  password: string;
}

// Response of POST /api/v1/Auth/Login.
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

// The bits of the signed-in user we keep around, decoded from the JWT.
export interface CurrentUser {
  id: string; // the "sub" claim = Identity user id (used as reservation userId)
  username: string;
  email: string | null;
  roles: string[];
}
