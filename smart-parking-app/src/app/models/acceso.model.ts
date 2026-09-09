export interface ValidarAccesoResponse {
  valido: boolean;
  mensaje: string;
  conductorNombre?: string;
  plazaCodigo?: string;
  tipoAccesoRegistrado?: 'entrada' | 'salida';
}

export interface RegistrarSalidaResponse {
  montoCobrado: number;
  duracionMinutos: number;
  metodoPago: string;
}

export type MetodoPago = 'efectivo' | 'tarjeta' | 'transferencia';
