export interface User {
  id: string;
  username: string;
  email: string;
  role: 'Player' | 'Master' | 'Admin';
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  expiresAt: string;
}

export interface ApiResponse<T> {
  Success: boolean;
  Data: T;
  Message: string;
}

export interface AuthResponseData {
  token: string;
  refreshToken: string;
  user: User;
  expires: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface UpdateUserRequest {
  username?: string;
  email?: string;
}