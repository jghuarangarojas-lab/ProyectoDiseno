export type UserRole =
  | 'ADMIN'
  | 'BOTANIST'
  | 'GARDENER'
  | 'CASHIER'
  | 'WAREHOUSE'
  | 'DELIVERY';

export interface AuthUser {
  username: string;
  email: string;
  role: UserRole;
  expiresAtUtc: string;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  email: string;
  role: UserRole;
  expiresAtUtc: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  token: string;
  newPassword: string;
}
