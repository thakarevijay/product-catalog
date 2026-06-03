import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';

      if (error.error?.detail) {
        errorMessage = error.error.detail;
      } else if (error.error?.title) {
        errorMessage = error.error.title;
      } else if (error.status === 0) {
        errorMessage = 'Cannot connect to server. Please check if the API is running.';
      } else if (error.status === 404) {
        errorMessage = 'Resource not found';
      } else if (error.status === 400) {
        errorMessage = 'Validation failed. Please check your input.';
      }

      console.error('HTTP Error:', error);
      return throwError(() => ({ message: errorMessage, errors: error.error?.errors }));
    })
  );
};
