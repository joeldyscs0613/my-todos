import { jwtDecode } from 'jwt-decode';
import type { DecodedToken, User } from '../types/auth.types';

export const decodeToken = (token: string): DecodedToken | null => {
  try {
    return jwtDecode<DecodedToken>(token);
  } catch (error) {
    console.error('Failed to decode token:', error);
    return null;
  }
};

export const isTokenExpired = (token: string): boolean => {
  const decoded = decodeToken(token);
  if (!decoded) return true;

  const currentTime = Date.now() / 1000;
  return decoded.exp < currentTime;
};

export const getUserFromToken = (token: string): User | null => {
  const decoded = decodeToken(token);
  if (!decoded) return null;

  return {
    id: decoded.sub,
    email: decoded.email,
    firstName: decoded.given_name,
    lastName: decoded.family_name,
    roles: Array.isArray(decoded.role) ? decoded.role : decoded.role ? [decoded.role] : [],
    tenantId: decoded.tenant_id,
  };
};
