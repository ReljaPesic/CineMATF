// Builds unsigned JWTs for tests 
// Claim keys Identity.API's TokenService writes 
export const NAME_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
export const NAMEID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
export const EMAIL_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress';
export const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

// base64url with no padding
function b64url(value: unknown): string {
  return btoa(JSON.stringify(value)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

// wrap claims object into a `header.payload.sig` string
export function fakeJwt(claims: Record<string, unknown>): string {
  return `${b64url({ alg: 'HS256', typ: 'JWT' })}.${b64url(claims)}.signature`;
}

export interface JwtUserOptions {
  sub?: string;
  name?: string;
  email?: string;
  roles?: string[] | string;
  cardNumber?: string;
  // negative produces an already-expired token
  expiresInSeconds?: number;
}

// builds a token that carries the claims AuthService.decode() reads
export function jwtForUser(options: JwtUserOptions = {}): string {
  const {
    sub = 'u1',
    name = 'alice',
    email = 'alice@cinematf.local',
    roles = ['User'],
    cardNumber = '4111111111111111',
    expiresInSeconds = 3600,
  } = options;

  return fakeJwt({
    sub,
    [NAME_CLAIM]: name,
    email,
    [ROLE_CLAIM]: roles,
    cardNumber,
    exp: Math.floor(Date.now() / 1000) + expiresInSeconds,
  });
}
