export interface PuntoRuta {
  latitud: number;
  longitud: number;
  nombreNodo: string;
}

export interface TramoRuta {
  nombreCalle: string;
  distanciaMetros: number;
  esCalleSaturada: boolean;
}

export type AlgoritmoRuta = 'astar' | 'dijkstra';

export interface RutaResponse {
  algoritmo: AlgoritmoRuta;
  puntos: PuntoRuta[];
  tramos: TramoRuta[];
  distanciaTotalMetros: number;
  tiempoEstimadoMinutos: number;
  nodosExplorados: number;
}
