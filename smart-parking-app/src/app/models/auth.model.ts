export type RolUsuario = 'conductor' | 'administrador' | 'operador';

export interface Usuario {
  id: number;
  nombre: string;
  correo: string;
  cedula: string;
  rol: RolUsuario;
}

export interface LoginRequest {
  identificador: string; // correo o cédula
  password: string;
}

export interface LoginResponse {
  token: string;
  expiraEn: string; // ISO date
  usuario: Usuario;
}

export interface RegistroRequest {
  nombre: string;
  cedula: string;
  correo: string;
  password: string;
}
