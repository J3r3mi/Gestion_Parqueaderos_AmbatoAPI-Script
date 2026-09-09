import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.estaAutenticado()) {
    return true;
  }

  router.navigate(['/login']);
  return false;
};

/** Guard adicional para rutas exclusivas del Administrador (ej. /dashboard). */
export const rolAdministradorGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const usuario = authService.usuarioActual();

  if (usuario?.rol === 'administrador') {
    return true;
  }

  router.navigate(['/home']);
  return false;
};

/** Guard para rutas operativas de Garita/Acceso (Operador y Administrador). */
export const rolPersonalGaritaGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const usuario = authService.usuarioActual();

  if (usuario?.rol === 'operador' || usuario?.rol === 'administrador') {
    return true;
  }

  router.navigate(['/home']);
  return false;
};
