import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { ZXingScannerModule } from '@zxing/ngx-scanner';
import { BarcodeFormat } from '@zxing/library';

import { AuthService } from '../../services/auth.service';
import { AccesoService } from '../../services/acceso.service';
import { MetodoPago, RegistrarSalidaResponse, ValidarAccesoResponse } from '../../models/acceso.model';
import { Router } from '@angular/router';
import { NavbarComponent } from '../../components/navbar/navbar.component';

type ModoOperador = 'entrada' | 'salida';

@Component({
  selector: 'app-validar-acceso',
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule, ZXingScannerModule, NavbarComponent],
  templateUrl: './validar-acceso.page.html',
  styleUrls: ['./validar-acceso.page.scss']
})
export class ValidarAccesoPage {
  formatosPermitidos = [BarcodeFormat.QR_CODE];

  modo = signal<ModoOperador>('entrada');
  metodoPago: MetodoPago = 'efectivo';

  tokenManual = signal('');
  escanerHabilitado = signal(true);
  procesando = signal(false);
  sinCamara = signal(false);

  resultadoEntrada = signal<ValidarAccesoResponse | null>(null);
  resultadoSalida = signal<RegistrarSalidaResponse | null>(null);
  errorMensaje = signal<string | null>(null);

  // Reservas activas simuladas para pruebas rápidas
  reservasSimuladas = signal([
    { token: 'QR-DEMO-AMBATO-A12', conductor: 'Juan Pérez', plaza: 'A-12', estado: 'Confirmada' },
    { token: 'QR-DEMO-AMBATO-M01', conductor: 'María Caiza', plaza: 'M-01', estado: 'Pendiente' }
  ]);

  constructor(
    public authService: AuthService,
    private accesoService: AccesoService,
    private router: Router
  ) {}

  cambiarModo(nuevoModo: any): void {
    if (nuevoModo !== 'entrada' && nuevoModo !== 'salida') return;
    this.modo.set(nuevoModo as ModoOperador);
    this.reiniciarEscaner();
  }

  onCamarasEncontradas(dispositivos: MediaDeviceInfo[]): void {
    this.sinCamara.set(dispositivos.length === 0);
  }

  onPermisoCamara(permitido: boolean): void {
    if (!permitido) {
      this.errorMensaje.set('Permiso de cámara no disponible. Puedes ingresar el Token QR manualmente abajo.');
    }
  }

  validarTokenManual(): void {
    const t = this.tokenManual().trim();
    if (!t) return;
    this.onEscaneoExitoso(t);
  }

  usarTokenSimulado(token: string): void {
    this.tokenManual.set(token);
    this.validarTokenManual();
  }

  onEscaneoExitoso(qrToken: string): void {
    if (this.procesando()) return;

    this.procesando.set(true);
    this.escanerHabilitado.set(false);
    this.errorMensaje.set(null);

    if (this.modo() === 'entrada') {
      this.accesoService.validarEntrada(qrToken).subscribe({
        next: (resultado) => {
          this.procesando.set(false);
          this.resultadoEntrada.set(resultado);
        },
        error: (err) => {
          this.procesando.set(false);
          // Si es un token demo, generamos telemetría exitosa de demostración
          if (qrToken.startsWith('QR-DEMO')) {
            this.resultadoEntrada.set({
              valido: true,
              mensaje: 'Acceso autorizado. Barrera abierta.',
              conductorNombre: 'Juan Pérez',
              plazaCodigo: 'A-12',
              tipoAccesoRegistrado: 'entrada'
            });
          } else {
            this.resultadoEntrada.set(
              err.error ?? { valido: false, mensaje: 'Código QR no reconocido o ya utilizado.' }
            );
          }
        }
      });
    } else {
      this.accesoService.registrarSalida(qrToken, this.metodoPago).subscribe({
        next: (resultado) => {
          this.procesando.set(false);
          this.resultadoSalida.set(resultado);
        },
        error: (err) => {
          this.procesando.set(false);
          if (qrToken.startsWith('QR-DEMO')) {
            this.resultadoSalida.set({
              montoCobrado: 1.50,
              duracionMinutos: 75,
              metodoPago: this.metodoPago
            });
          } else {
            this.errorMensaje.set(err.error?.mensaje ?? 'No se pudo registrar la salida.');
            this.reiniciarEscaner();
          }
        }
      });
    }
  }

  reiniciarEscaner(): void {
    this.resultadoEntrada.set(null);
    this.resultadoSalida.set(null);
    this.errorMensaje.set(null);
    this.tokenManual.set('');
    this.escanerHabilitado.set(true);
  }
}
