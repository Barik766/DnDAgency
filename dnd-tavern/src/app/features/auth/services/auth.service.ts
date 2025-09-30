import { Injectable } from '@angular/core';
import { Observable, tap, catchError, of, map } from 'rxjs';
import { 
  User, 
  LoginRequest, 
  RegisterRequest, 
  ChangePasswordRequest, 
  UpdateUserRequest, 
  ApiResponse, 
  AuthResponseData 
} from '../interfaces/auth.interface';
import { TokenService } from './token.service';
import { UserStateService } from './user-state.service';
import { AuthApiService } from './auth-api.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(
    private tokenService: TokenService,
    private userStateService: UserStateService,
    private authApiService: AuthApiService
  ) {
    this.initializeAuthState();
  }

  get currentUser() {
    return this.userStateService.currentUser;
  }

  get isAuthenticated() {
    return this.userStateService.isAuthenticated;
  }

  login(credentials: LoginRequest): Observable<ApiResponse<AuthResponseData>> {
    return this.authApiService.login(credentials).pipe(
      tap(response => {
        if (response.Success && response.Data) {
          this.setAuthData(response.Data);
        }
      })
    );
  }

  loginWithGoogle(idToken: string): Observable<ApiResponse<AuthResponseData>> {
    return this.authApiService.googleLogin(idToken).pipe(
      tap(response => {
        if (response.Success && response.Data) {
          this.setAuthData(response.Data);
        }
      })
    );
  }

  loginWithGoogleCode(code: string): Observable<ApiResponse<AuthResponseData>> {
    return this.authApiService.googleLoginWithCode(code).pipe(
      tap(response => {
        if (response.Success && response.Data) {
          this.setAuthData(response.Data);
        }
      })
    );
  }

  register(userData: RegisterRequest): Observable<User> {
    return this.authApiService.register(userData);
  }

  logout(): void {
    this.tokenService.removeToken();
    this.tokenService.removeRefreshToken(); 
    this.userStateService.clearUser();
  }

  getCurrentUser(): Observable<User> {
    return this.authApiService.getCurrentUser();
  }

  changePassword(passwordData: ChangePasswordRequest): Observable<{ message: string }> {
    return this.authApiService.changePassword(passwordData);
  }

  updateProfile(userId: string, userData: UpdateUserRequest): Observable<User> {
    return this.authApiService.updateProfile(userId, userData).pipe(
      tap(user => this.userStateService.setUser(user))
    );
  }

  deactivateAccount(userId: string): Observable<void> {
    return this.authApiService.deactivateAccount(userId).pipe(
      tap(() => this.logout())
    );
  }

  getToken(): string | null {
    return this.tokenService.getToken();
  }

  isTokenValid(): boolean {
    return this.tokenService.isTokenValid();
  }

  refreshUserData(): Observable<User | null> {
    return this.getCurrentUser().pipe(
      tap(freshUser => {
        if (freshUser) {
          this.userStateService.setUser(freshUser);
        }
      }),
      catchError(error => {
        console.error('Не удалось загрузить свежие данные пользователя:', error);
        return of(null);
      })
    );
  }

  private initializeAuthState(): void {
    const token = this.tokenService.getToken();
    
    if (!token || !this.tokenService.isTokenValid(token)) {
      this.logout();
      return;
    }

    try {
      const userFromToken = this.tokenService.extractUserFromToken(token);
      this.userStateService.setUser(userFromToken);
      
      // Получаем свежие данные в фоне
      //this.refreshUserData().subscribe();
    } catch (error) {
      console.error('Ошибка при чтении токена:', error);
      this.logout();
    }
  }

  private setAuthData(authData: AuthResponseData): void {
  this.tokenService.setToken(authData.token);
  this.tokenService.setRefreshToken(authData.refreshToken); 
  this.userStateService.setUser(authData.user);
}

  refreshToken(): Observable<boolean> {
    const refreshToken = this.tokenService.getRefreshToken(); 
    if (!refreshToken) return of(false);
    
    return this.authApiService.refreshToken(refreshToken).pipe(
      tap(response => {
        if (response.Success && response.Data) {
          this.setAuthData(response.Data);
        }
      }),
      map(() => true),
      catchError(err => {
        this.logout();
        return of(false);
      })
    );
  }

  


}
