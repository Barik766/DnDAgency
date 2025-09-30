import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';


// Базовый guard для проверки аутентификации
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated() && authService.isTokenValid()) {
    return true;
  }

  // Перенаправляем на страницу входа
  router.navigate(['/login'], { 
    queryParams: { returnUrl: state.url } 
  });
  return false;
};

// Guard для проверки ролей
export const roleGuard = (allowedRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    
    if (!authService.isAuthenticated() || !authService.isTokenValid()) {
      router.navigate(['/login'], { 
        queryParams: { returnUrl: state.url } 
      });
      return false;
    }

    const currentUser = authService.currentUser();
    if (!currentUser || !allowedRoles.includes(currentUser.role)) {
      router.navigate(['/forbidden']);
      return false;
    }

    return true;
  };
};

// Guard для GM
export const gmGuard: CanActivateFn = roleGuard(['Master', 'Admin']);

// Guard для Admin
export const adminGuard: CanActivateFn = roleGuard(['Admin']);

// Guard для гостей (неавторизованных пользователей)
export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return true;
  }

  // Если пользователь уже авторизован, перенаправляем на главную
  router.navigate(['/']);
  return false;
};