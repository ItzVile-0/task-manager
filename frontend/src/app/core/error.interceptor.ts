import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const snackBar = inject(MatSnackBar);

  return next(req).pipe(
    catchError((err: HttpErrorResponse) => {
      snackBar.open(errorMessage(err), 'Cerrar', { duration: 6000, panelClass: 'snack-error' });
      return throwError(() => err);
    }),
  );
};

// la api responde con ProblemDetails: "errors" en validaciones, "detail" en reglas de negocio
export function errorMessage(err: HttpErrorResponse): string {
  if (err.status === 0) return 'No se pudo conectar con el servidor';

  const body = err.error;
  if (body?.errors) return Object.values(body.errors).flat().join('. ');

  return body?.detail ?? body?.title ?? 'Ocurrió un error inesperado';
}
