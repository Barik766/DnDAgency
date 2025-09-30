import { Injectable, signal, computed } from '@angular/core';
import { User } from '../interfaces/auth.interface';

@Injectable({ providedIn: 'root' })
export class UserStateService {
  private _currentUser = signal<User | null>(null);
  private _isAuthenticated = computed(() => this._currentUser() !== null);

  get currentUser() {
    return this._currentUser.asReadonly();
  }

  get isAuthenticated() {
    return this._isAuthenticated;
  }

  setUser(user: User | null): void {
    this._currentUser.set(user);
  }

  updateUser(updates: Partial<User>): void {
    const current = this._currentUser();
    if (current) {
      this._currentUser.set({ ...current, ...updates });
    }
  }

  clearUser(): void {
    this._currentUser.set(null);
  }

  getUserId(): string | null {
    return this._currentUser()?.id || null;
  }

  isUserRole(role: 'Player' | 'Master' | 'Admin'): boolean {
  const user = this._currentUser();
  if (!user) return false;
  
  return user.role === role;
}
}