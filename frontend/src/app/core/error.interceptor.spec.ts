import { HttpErrorResponse } from '@angular/common/http';
import { errorMessage } from './error.interceptor';

describe('errorMessage', () => {
  it('avisa cuando la api no responde', () => {
    expect(errorMessage(new HttpErrorResponse({ status: 0 }))).toBe('No se pudo conectar con el servidor');
  });

  it('junta los errores de validacion', () => {
    const err = new HttpErrorResponse({
      status: 400,
      error: { errors: { Title: ['El título es obligatorio'], UserId: ['La tarea debe tener un usuario asignado'] } },
    });

    expect(errorMessage(err)).toBe('El título es obligatorio. La tarea debe tener un usuario asignado');
  });

  it('usa el detail de las reglas de negocio', () => {
    const err = new HttpErrorResponse({ status: 422, error: { title: 'Regla de negocio', detail: 'No se puede' } });

    expect(errorMessage(err)).toBe('No se puede');
  });
});
