import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { MetodoPago, RegistrarSalidaResponse, ValidarAccesoResponse } from '../models/acceso.model';

@Injectable({ providedIn: 'root' })
export class AccesoService {
  constructor(private http: HttpClient) {}

  validarEntrada(qrToken: string): Observable<ValidarAccesoResponse> {
    return this.http.post<ValidarAccesoResponse>(
      `${environment.apiUrl}/accesos/validar-entrada`,
      { qrToken }
    );
  }

  registrarSalida(qrToken: string, metodoPago: MetodoPago): Observable<RegistrarSalidaResponse> {
    return this.http.post<RegistrarSalidaResponse>(
      `${environment.apiUrl}/accesos/registrar-salida?metodoPago=${metodoPago}`,
      { qrToken }
    );
  }
}
