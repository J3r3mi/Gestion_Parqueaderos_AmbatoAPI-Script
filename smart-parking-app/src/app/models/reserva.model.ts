export type EstadoReserva = 'pendiente' | 'confirmada' | 'cancelada' | 'expirada' | 'completada';

export interface CrearReservaRequest {
  plazaId: number;
  horaEstimadaArribo: string; // ISO date
}

export interface ReservaResponse {
  id: number;
  plazaCodigo: string;
  parqueaderoNombre: string;
  horaReserva: string;
  horaEstimadaArribo: string;
  estado: EstadoReserva;
  qrToken: string | null;
  qrExpiraEn: string | null;
}
