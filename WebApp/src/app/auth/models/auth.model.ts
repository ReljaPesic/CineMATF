// Body of POST /api/v1/Auth/Login (Identity.API).
export interface LoginRequest {
  userName: string;
  password: string;
}

// Body of POST /api/v1/Auth/RegisterUser.
export interface RegisterRequest {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  password: string;
  cardNumber: string;
  phoneNumber: string | null;
}

// Response of POST /api/v1/Auth/Login and /api/v1/Auth/Refresh.
export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

// Body of POST /api/v1/Auth/Refresh.
export interface RefreshTokenRequest {
  userName: string;
  refreshToken: string;
}

// The bits of the signed-in user we keep around, decoded from the JWT.
export interface CurrentUser {
  id: string; // the "sub" claim = Identity user id (used as reservation userId)
  username: string;
  email: string | null;
  roles: string[];
}
