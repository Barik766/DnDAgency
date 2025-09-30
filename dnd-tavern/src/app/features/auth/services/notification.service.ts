import { Injectable, signal } from '@angular/core';

export interface Notification {
  message: string;
  type: 'success' | 'error' | 'info' | 'warning';
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private _notification = signal<Notification | null>(null);

  get notification() {
    return this._notification.asReadonly();
  }

  showSuccess(message: string, duration = 3000): void {
    this.show({ message, type: 'success', duration });
  }

  showError(message: string, duration = 5000): void {
    this.show({ message, type: 'error', duration });
  }

  showInfo(message: string, duration = 3000): void {
    this.show({ message, type: 'info', duration });
  }

  showWarning(message: string, duration = 4000): void {
    this.show({ message, type: 'warning', duration });
  }

  private show(notification: Notification): void {
    this._notification.set(notification);
    
    if (notification.duration && notification.duration > 0) {
      setTimeout(() => this.clear(), notification.duration);
    }
  }

  clear(): void {
    this._notification.set(null);
  }
}