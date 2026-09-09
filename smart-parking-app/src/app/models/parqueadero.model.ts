export interface ParqueaderoListItem {
  id: number;
  nombre: string;
  direccion: string;
  latitud: number;
  longitud: number;
  capacidadTotal: number;
  cuposLibres: number;
  plazasOcupadas?: number;
}

export interface Plaza {
  id: number;
  codigo: string;
  estado: 'libre' | 'reservada' | 'ocupada' | 'mantenimiento';
  tipoVehiculo: 'auto' | 'moto' | 'camioneta';
}

export interface Tarifa {
  id: number;
  tipoVehiculo: 'auto' | 'moto' | 'camioneta';
  valorHora: number;
  vigenteDesde: string;
}
