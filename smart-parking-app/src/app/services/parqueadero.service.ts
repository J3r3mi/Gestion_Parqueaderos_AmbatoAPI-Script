import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ParqueaderoListItem, Plaza, Tarifa } from '../models/parqueadero.model';

@Injectable({ providedIn: 'root' })
export class ParqueaderoService {
  constructor(private http: HttpClient) {}

  listar(): Observable<ParqueaderoListItem[]> {
    return this.http.get<ParqueaderoListItem[]>(`${environment.apiUrl}/parqueaderos`);
  }

  listarPlazas(parqueaderoId: number): Observable<Plaza[]> {
    return this.http.get<Plaza[]>(`${environment.apiUrl}/parqueaderos/${parqueaderoId}/plazas`);
  }

  listarTarifas(parqueaderoId: number): Observable<Tarifa[]> {
    return this.http.get<Tarifa[]>(`${environment.apiUrl}/parqueaderos/${parqueaderoId}/tarifas`);
  }
}
