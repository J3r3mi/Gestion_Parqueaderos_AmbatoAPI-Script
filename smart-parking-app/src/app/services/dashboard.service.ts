import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { forkJoin, Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Alerta,
  KpiGeneral,
  KpiPorParqueadero,
  OcupacionHoraria,
  RecaudacionDiaria
} from '../models/dashboard.model';

export interface DashboardCompleto {
  kpis: KpiGeneral;
  kpisPorParqueadero: KpiPorParqueadero[];
  ocupacionHoraria: OcupacionHoraria[];
  alertas: Alerta[];
  recaudacion: RecaudacionDiaria;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly base = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  kpis(): Observable<KpiGeneral> {
    return this.http.get<KpiGeneral>(`${this.base}/kpis`);
  }

  kpisPorParqueadero(): Observable<KpiPorParqueadero[]> {
    return this.http.get<KpiPorParqueadero[]>(`${this.base}/kpis-por-parqueadero`);
  }

  ocupacionHoraria(): Observable<OcupacionHoraria[]> {
    return this.http.get<OcupacionHoraria[]>(`${this.base}/ocupacion-horaria`);
  }

  alertas(): Observable<Alerta[]> {
    return this.http.get<Alerta[]>(`${this.base}/alertas`);
  }

  recaudacion(): Observable<RecaudacionDiaria> {
    return this.http.get<RecaudacionDiaria>(`${this.base}/recaudacion`);
  }

  /** Trae los 5 endpoints en paralelo — una sola suscripción para refrescar todo el dashboard. */
  cargarTodo(): Observable<DashboardCompleto> {
    return forkJoin({
      kpis: this.kpis(),
      kpisPorParqueadero: this.kpisPorParqueadero(),
      ocupacionHoraria: this.ocupacionHoraria(),
      alertas: this.alertas(),
      recaudacion: this.recaudacion()
    });
  }
}
