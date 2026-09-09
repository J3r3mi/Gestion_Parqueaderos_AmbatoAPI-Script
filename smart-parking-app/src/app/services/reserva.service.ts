import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CrearReservaRequest, ReservaResponse } from '../models/reserva.model';

@Injectable({ providedIn: 'root' })
export class ReservaService {
  constructor(private http: HttpClient) {}

  crear(request: CrearReservaRequest): Observable<ReservaResponse> {
    return this.http.post<ReservaResponse>(`${environment.apiUrl}/reservas`, request);
  }

  cancelar(reservaId: number): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(
      `${environment.apiUrl}/reservas/${reservaId}/cancelar`,
      {}
    );
  }

  misReservas(): Observable<ReservaResponse[]> {
    return this.http.get<ReservaResponse[]>(`${environment.apiUrl}/reservas/mias`);
  }
}
