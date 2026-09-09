import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse, RegistroRequest, Usuario } from '../models/auth.model';

const TOKEN_KEY = 'smart_parking_token';
const USER_KEY = 'smart_parking_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Signal reactivo: los componentes pueden leer el usuario actual sin suscribirse manualmente.
  usuarioActual = signal<Usuario | null>(this.leerUsuarioGuardado());

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiUrl}/auth/login`, request)
      .pipe(
        tap((respuesta) => {
          localStorage.setItem(TOKEN_KEY, respuesta.token);
          localStorage.setItem(USER_KEY, JSON.stringify(respuesta.usuario));
          this.usuarioActual.set(respuesta.usuario);
        })
      );
  }

  registro(request: RegistroRequest): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(`${environment.apiUrl}/auth/registro`, request);
  }

  solicitarRecuperacion(identificador: string): Observable<{ mensaje: string; tokenSoloParaDemo?: string }> {
    return this.http.post<{ mensaje: string; tokenSoloParaDemo?: string }>(
      `${environment.apiUrl}/auth/solicitar-recuperacion`,
      { identificador }
    );
  }

  restablecerPassword(token: string, nuevaPassword: string): Observable<{ mensaje: string }> {
    return this.http.post<{ mensaje: string }>(`${environment.apiUrl}/auth/restablecer-password`, {
      token,
      nuevaPassword
    });
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.usuarioActual.set(null);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  estaAutenticado(): boolean {
    return !!this.getToken();
  }

  private leerUsuarioGuardado(): Usuario | null {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? (JSON.parse(raw) as Usuario) : null;
  }
}
