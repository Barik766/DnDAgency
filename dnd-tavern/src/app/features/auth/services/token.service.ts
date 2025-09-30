import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { User } from '../interfaces/auth.interface';

export interface TokenPayload {
  [key: string]: any;
  nameid?: string;
  unique_name?: string;
  email?: string;
  role?: string;
  exp?: number;
}

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly TOKEN_KEY = 'tavern_token';
  private readonly REFRESH_TOKEN_KEY = 'tavern_refresh_token';
  
  private readonly JWT_CLAIMS = {
    NameIdentifier: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
    Name: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
    Email: 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
    Role: 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
  };

  constructor(@Inject(PLATFORM_ID) private platformId: Object) {}

  getToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) return null;
    return localStorage.getItem(this.TOKEN_KEY);
  }

  setToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.TOKEN_KEY, token);
    }
  }

  removeToken(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.TOKEN_KEY);
    }
  }

  isTokenValid(token?: string): boolean {
    const currentToken = token || this.getToken();
    if (!currentToken) return false;

    const payload = this.decodeToken(currentToken);
    if (!payload?.exp) return false;

    return Date.now() < payload.exp * 1000;
  }

  private base64UrlDecode(input: string): string {
    input = input.replace(/-/g, '+').replace(/_/g, '/');
    const pad = input.length % 4;
    if (pad) {
      input += '='.repeat(4 - pad);
    }
    try {
      return atob(input);
    } catch {
      return '';
    }
  }

  decodeToken(token?: string): TokenPayload | null {
    const currentToken = token || this.getToken();
    if (!currentToken) return null;

    try {
      const base64Payload = currentToken.split('.')[1];
      if (!base64Payload) return null;

      const decoded = this.base64UrlDecode(base64Payload);
      if (!decoded) return null;

      return JSON.parse(decoded);
    } catch {
      return null;
    }
  }

  extractUserFromToken(token?: string): User | null {
    const payload = this.decodeToken(token);
    if (!payload) return null;

    return {
      id: payload[this.JWT_CLAIMS.NameIdentifier] || payload.nameid || '',
      username: payload[this.JWT_CLAIMS.Name] || payload.unique_name || '',
      email: payload[this.JWT_CLAIMS.Email] || payload.email || '',
      role: (payload[this.JWT_CLAIMS.Role] || payload.role || 'Player') as 'Player' | 'Master' | 'Admin',
      isActive: true,
      createdAt: '',
      updatedAt: ''
    };
  }

  getRefreshToken(): string | null {
    if (!isPlatformBrowser(this.platformId)) return null;
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  setRefreshToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.REFRESH_TOKEN_KEY, token);
    }
  }

  removeRefreshToken(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    }
  }
}
