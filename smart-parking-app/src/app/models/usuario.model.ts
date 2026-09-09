export type RolStaff = 'administrador' | 'operador';

export interface UsuarioListItem {
  id: number;
  nombre: string;
  correo: string;
  cedula: string;
  rol: string;
  activo: boolean;
  createdAt: string;
}

export interface RegistrarStaffRequest {
  nombre: string;
  cedula: string;
  correo: string;
  password: string;
  rol: RolStaff;
}
