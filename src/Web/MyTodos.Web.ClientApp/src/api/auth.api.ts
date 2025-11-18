import apiClient from './client';
import type { LoginRequest, RegisterRequest, AuthResponse } from '../types/auth.types';

export const authApi = {
  login: async (credentials: LoginRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/api/identity/auth/login', credentials);
    return response.data;
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    const response = await apiClient.post<AuthResponse>('/api/identity/auth/register', data);
    return response.data;
  },

  logout: async (): Promise<void> => {
    // Optional: call backend to invalidate token
    // await apiClient.post('/api/identity/auth/logout');
    localStorage.removeItem('accessToken');
    localStorage.removeItem('user');
  },
};
