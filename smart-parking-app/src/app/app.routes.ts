import { Routes } from '@angular/router';
import { authGuard, rolAdministradorGuard, rolPersonalGaritaGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.page').then((m) => m.LoginPage)
  },
  {
    path: 'registro',
    loadComponent: () => import('./pages/registro/registro.page').then((m) => m.RegistroPage)
  },
  {
    path: 'recuperar-password',
    loadComponent: () =>
      import('./pages/recuperar-password/recuperar-password.page').then((m) => m.RecuperarPasswordPage)
  },
  {
    path: 'home',
    loadComponent: () => import('./pages/home/home.page').then((m) => m.HomePage),
    canActivate: [authGuard]
  },
  {
    path: 'mis-reservas',
    loadComponent: () =>
      import('./pages/mis-reservas/mis-reservas.page').then((m) => m.MisReservasPage),
    canActivate: [authGuard]
  },
  {
    path: 'reservar/:id',
    loadComponent: () => import('./pages/reservar/reservar.page').then((m) => m.ReservarPage),
    canActivate: [authGuard]
  },
  {
    path: 'gestion-usuarios',
    loadComponent: () =>
      import('./pages/gestion-usuarios/gestion-usuarios.page').then((m) => m.GestionUsuariosPage),
    canActivate: [authGuard, rolAdministradorGuard]
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.page').then((m) => m.DashboardPage),
    canActivate: [authGuard, rolAdministradorGuard]
  },
  {
    path: 'validar-acceso',
    loadComponent: () =>
      import('./pages/validar-acceso/validar-acceso.page').then((m) => m.ValidarAccesoPage),
    canActivate: [authGuard, rolPersonalGaritaGuard]
  },
  { path: '**', redirectTo: 'login' }
];
