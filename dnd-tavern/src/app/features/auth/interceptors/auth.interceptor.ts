import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Проверяем специальный заголовок для пропуска интерцептора
  if (req.headers.has('Skip-Auth-Interceptor')) {
    const newReq = req.clone({
      headers: req.headers.delete('Skip-Auth-Interceptor')
    });
    return next(newReq);
  }

  // эндпоинты, куда токен не нужен
  const publicEndpoints = [
    '/login',
    '/register',
    '/campaigns/catalog',
    '/campaigns/upcoming-games',
    '/users/me',
    '/users/refresh-token',
    '/home',
    '/campaigns/id',
  ];

  if (req.method === 'GET' && req.url.toLowerCase().includes('/api/masters')) {
    return next(req);
  }

  if (publicEndpoints.some(url => req.url.toLowerCase().includes(url.toLowerCase()))) {
    return next(req);
  }

  const token = authService.getToken();

  const handleRequest = (accessToken: string | null) => {
    let authReq = req;
    if (accessToken) {
      authReq = req.clone({ setHeaders: { Authorization: `Bearer ${accessToken}` } });
    }

    return next(authReq).pipe(
      catchError(err => {
        if (err.status === 401) {
          authService.logout();
          router.navigate(['/login']);
        }
        return throwError(() => err);
      })
    );
  };

  if (!token || !authService.isTokenValid()) {
    return authService.refreshToken().pipe(
      switchMap(success => {
        if (success) {
          return handleRequest(authService.getToken());
        } else {
          authService.logout();
          router.navigate(['/login']);
          return handleRequest(null);
        }
      }),
      catchError(() => {
        authService.logout();
        router.navigate(['/login']);
        return handleRequest(null);
      })
    );
  }

  return handleRequest(token);
};