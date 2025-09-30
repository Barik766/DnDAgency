import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UserStateService } from '../../../features/auth/services/user-state.service';
import { AuthService } from '../../../features/auth/services/auth.service';

@Component({
  selector: 'app-header',
  imports: [CommonModule, RouterModule],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header {
  private userState = inject(UserStateService);
  private authService = inject(AuthService);
  private router = inject(Router);

  get isAuthenticated() {
    return this.userState.isAuthenticated();
  }

  get isMaster() {
    return this.userState.isUserRole('Master') || this.userState.isUserRole('Admin');
  }


  onLogin() {
    this.router.navigate(['/login']);
  }

  onLogout() {
    this.authService.logout();
    this.userState.clearUser();
    this.router.navigate(['/home']);
  }

  goToProfile() {
    this.router.navigate(['/masters/me']);
  }
}
