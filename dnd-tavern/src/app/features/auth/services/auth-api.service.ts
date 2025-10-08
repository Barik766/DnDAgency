import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  User,
  LoginRequest,
  RegisterRequest,
  ChangePasswordRequest,
  UpdateUserRequest,
  ApiResponse,
  AuthResponseData
} from '../interfaces/auth.interface';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly API_URL = '/api/users'; 

  constructor(private http: HttpClient) {}

  login(credentials: LoginRequest): Observable<ApiResponse<AuthResponseData>> {
    return this.http.post<ApiResponse<AuthResponseData>>(`${this.API_URL}/login`, credentials);
  }

  googleLogin(idToken: string): Observable<ApiResponse<AuthResponseData>> {
    return this.http.post<ApiResponse<AuthResponseData>>(`${this.API_URL}/google-login`, { idToken });
  }

  googleLoginWithCode(code: string): Observable<ApiResponse<AuthResponseData>> {
    return this.http.post<ApiResponse<AuthResponseData>>(`${this.API_URL}/google-callback`, { 
      code,
      redirectUri: window.location.origin 
    });
  }

  register(userData: RegisterRequest): Observable<User> {
    return this.http.post<User>(`${this.API_URL}/register`, userData);
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.API_URL}/me`);
  }

  changePassword(passwordData: ChangePasswordRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.API_URL}/change-password`, passwordData);
  }

  updateProfile(userId: string, userData: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.API_URL}/${userId}`, userData);
  }

  deactivateAccount(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${userId}`);
  }

  refreshToken(token: string): Observable<ApiResponse<AuthResponseData>> {
    return this.http.post<ApiResponse<AuthResponseData>>(`${this.API_URL}/refresh-token`, { token });
  }
}