import { Component, signal, OnInit, OnDestroy, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { LoginRequest, RegisterRequest } from '../interfaces/auth.interface';
import { AuthService } from '../services/auth.service';
import { FormValidationService } from '../services/form-validation.service';
import { NotificationService } from '../services/notification.service';

declare const google: any;

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth.component.html',
  styleUrl: './auth.component.scss'
})
export class AuthComponent implements OnInit, OnDestroy {
  private platformId = inject(PLATFORM_ID);
  
  isRegisterMode = signal(false);
  isLoading = signal(false);
  
  loginForm: FormGroup;
  registerForm: FormGroup;

  constructor(
    private authService: AuthService,
    private formValidation: FormValidationService,
    private notifications: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.formValidation.createLoginForm();
    this.registerForm = this.formValidation.createRegisterForm();
  }

  ngOnInit(): void {
    console.log('ngOnInit called, isPlatformBrowser:', isPlatformBrowser(this.platformId));
    if (isPlatformBrowser(this.platformId)) {
      console.log('Checking google object:', typeof google);
      this.initializeGoogleSignIn();
    }
  }

  ngOnDestroy(): void {
    // Cleanup if necessary
  }

  get notification() {
    return this.notifications.notification;
  }

  get currentForm(): FormGroup {
    return this.isRegisterMode() ? this.registerForm : this.loginForm;
  }

  getNotificationIcon(type: string): string {
    switch(type) {
      case 'success': return '✅';
      case 'error': return '⚠️';
      case 'warning': return '⚡';
      case 'info': return 'ℹ️';
      default: return '📢';
    }
  }

  getFieldError(fieldName: string): string | null {
    return this.formValidation.getFieldError(this.currentForm, fieldName);
  }

  onLogin(): void {
    if (!this.formValidation.isFormValid(this.loginForm) || this.isLoading()) {
      return;
    }

    this.setLoading(true);
    const credentials: LoginRequest = this.loginForm.value;

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.handleLoginSuccess(response);
      },
      error: (error) => {
        this.handleLoginError(error);
        this.setLoading(false);
      }
    });
  }

  onRegister(): void {
    if (!this.formValidation.isFormValid(this.registerForm) || this.isLoading()) {
      return;
    }

    this.setLoading(true);
    const userData: RegisterRequest = this.registerForm.value;

    this.authService.register(userData).subscribe({
      next: () => {
        this.handleRegisterSuccess();
        this.setLoading(false);
      },
      error: (error) => {
        this.handleRegisterError(error);
        this.setLoading(false);
      }
    });
  }

  toggleMode(): void {
    this.setLoading(false);
    this.isRegisterMode.set(!this.isRegisterMode());
    this.notifications.clear();
    this.resetForms();
  }

  signInWithGoogle(): void {
    // This method is called automatically via Google Button
  }

  private initializeGoogleSignIn(): void {
    const client = google.accounts.oauth2.initCodeClient({
      client_id: '198301924381-o64h8vn20o4f4qggqmtv26vov0andmgm.apps.googleusercontent.com',
      scope: 'email profile',
      ux_mode: 'popup',
      callback: (response: any) => {
        console.log('Google code received:', response);
        this.handleGoogleCodeCallback(response);
      }
    });

    const button = document.getElementById('google-signin-button');
    if (button) {
      button.onclick = () => client.requestCode();
    }
  }

  private handleGoogleCodeCallback(response: any): void {
    if (!response.code) {
      console.error('No code received from Google!');
      return;
    }

    this.setLoading(true);
    this.authService.loginWithGoogleCode(response.code).subscribe({
      next: (authResponse) => {
        this.handleLoginSuccess(authResponse);
      },
      error: (error) => {
        this.handleLoginError(error);
        this.setLoading(false);
      }
    });
  }


  private handleLoginSuccess(response: any): void {
    if (response.Success) {
      this.notifications.showSuccess('Welcome to the tavern! 🎉');
      const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
      setTimeout(() => {
        this.setLoading(false);
        this.router.navigate([returnUrl]);
      }, 1500);
    } else {
      this.notifications.showError(response.Message || 'Login error');
      this.setLoading(false);
    }
  }

  private handleLoginError(error: any): void {
    const message = this.extractErrorMessage(error, 'Invalid login or password. Please try again.');
    this.notifications.showError(message);
  }

  private handleRegisterSuccess(): void {
    this.notifications.showSuccess('Registration successful! Now you can enter the tavern.');
    this.isRegisterMode.set(false);
    this.registerForm.reset();
  }

  private handleRegisterError(error: any): void {
    const message = this.extractErrorMessage(error, 'Registration error. This email may already be in use.');
    this.notifications.showError(message);
  }

  private extractErrorMessage(error: any, defaultMessage: string): string {
    return error.error?.Message || 
           error.error?.message || 
           error.message || 
           defaultMessage;
  }

  private setLoading(loading: boolean): void {
    this.isLoading.set(loading);
  }

  private resetForms(): void {
    this.loginForm.reset();
    this.registerForm.reset();
    
    Object.keys(this.loginForm.controls).forEach(key => {
      this.loginForm.get(key)?.markAsUntouched();
    });
    Object.keys(this.registerForm.controls).forEach(key => {
      this.registerForm.get(key)?.markAsUntouched();
    });
  }
}