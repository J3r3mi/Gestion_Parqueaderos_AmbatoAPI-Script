import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { RegistrarStaffRequest, UsuarioListItem } from '../models/usuario.model';

@Injectable({ providedIn: 'root' })
export class UsuarioService {
  constructor(private http: HttpClient) {}

  listarStaff(): Observable<UsuarioListItem[]> {
    return this.http.get<UsuarioListItem[]>(`${environment.apiUrl}/usuarios`);
  }

  registrarStaff(request: RegistrarStaffRequest): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(
      `${environment.apiUrl}/auth/registrar-staff`,
      request
    );
  }

  actualizarEstado(usuarioId: number, activo: boolean): Observable<{ mensaje: string }> {
    return this.http.put<{ mensaje: string }>(
      `${environment.apiUrl}/usuarios/${usuarioId}/estado`,
      { activo }
    );
  }
}
