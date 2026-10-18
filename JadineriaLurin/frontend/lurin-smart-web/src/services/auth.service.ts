import { AUTH_API_BASE_URL } from '../config/api';
import { createHttpClient } from '../lib/httpClient';
import type {
  ForgotPasswordRequest,
  LoginRequest,
  LoginResponse,
  ResetPasswordRequest,
} from '../types/auth';

const client = createHttpClient({
  baseURL: AUTH_API_BASE_URL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const authService = {
  async login(request: LoginRequest): Promise<LoginResponse> {
    const { data } = await client.post<LoginResponse>('/api/auth/login', request);
    return data;
  },

  async forgotPassword(request: ForgotPasswordRequest): Promise<void> {
    await client.post('/api/auth/forgot-password', request);
  },

  async resetPassword(request: ResetPasswordRequest): Promise<void> {
    await client.post('/api/auth/reset-password', request);
  },
};
