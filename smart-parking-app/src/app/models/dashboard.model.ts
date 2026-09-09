export interface KpiGeneral {
  plazasTotales: number;
  plazasOcupadas: number;
  cuposLibres: number;
  reservasPendientesHoy: number;
}

export interface KpiPorParqueadero {
  parqueaderoId: number;
  nombre: string;
  plazasTotales: number;
  plazasOcupadas: number;
  cuposLibres: number;
  reservasActivas: number;
}

export interface OcupacionHoraria {
  hora: number; // 0-23
  entradas: number;
  salidas: number;
}

export interface Alerta {
  tipo: 'parqueadero_lleno' | 'reserva_vencida';
  mensaje: string;
  fechaHora: string;
}

export interface RecaudacionDiaria {
  fecha: string;
  total: number;
  transaccionesCount: number;
}
