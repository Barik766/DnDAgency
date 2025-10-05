import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

export const errorHandlerInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unknown error occurred';

      if (error.error instanceof ErrorEvent) {
        // Client-side or network error
        errorMessage = `Network error: ${error.error.message}`;
      } else if (error.status === 0) {
        // Status 0 usually indicates a network error:
        // 1. CORS (если preflight failed)
        // 2. Network timeout
        // 3. Server not responding
        // 4. API endpoint not found
        
        if (error.url) {
          errorMessage = `Unable to connect to server at ${error.url}. 
Possible causes:
- Server is not running
- Incorrect API endpoint
- Network issue
- CORS misconfiguration (check browser console for preflight errors)`;
        } else {
          errorMessage = 'Connection failed. Check your network or server status.';
        }
      } else if (error.status === 404) {
        errorMessage = `API endpoint not found: ${error.url}`;
      } else if (error.status === 500) {
        errorMessage = `Server error: ${error.message}`;
      } else if (error.error?.message) {
        // Structured error from backend
        errorMessage = error.error.message;
      } else if (typeof error.error === 'string') {
        errorMessage = error.error;
      } else {
        errorMessage = `HTTP ${error.status}: ${error.statusText || 'Unknown error'}`;
      }

      console.error('HTTP Error Details:', {
        status: error.status,
        statusText: error.statusText,
        url: error.url,
        message: errorMessage,
        error: error.error
      });

      return throwError(() => new Error(errorMessage));
    })
  );
};