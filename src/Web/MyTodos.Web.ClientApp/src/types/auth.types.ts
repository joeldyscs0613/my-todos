export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  token: string;
  refreshToken?: string;
  expiresAt: string;
}

export interface User {
  id: string;
  email: string;
  firstName?: string;
  lastName?: string;
  roles?: string[];
  tenantId?: string;
}

export interface DecodedToken {
  sub: string; // userId
  email: string;
  given_name?: string;
  family_name?: string;
  role?: string | string[];
  tenant_id?: string;
  exp: number;
  iss: string;
  aud: string;
}
