import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { IonicModule, AlertController, ToastController } from '@ionic/angular';
import * as QRCode from 'qrcode';

import { NavbarComponent } from '../../components/navbar/navbar.component';
import { ReservaService } from '../../services/reserva.service';
import { ReservaResponse } from '../../models/reserva.model';

@Component({
  selector: 'app-mis-reservas',
  standalone: true,
  imports: [CommonModule, IonicModule, RouterLink, NavbarComponent],
  templateUrl: './mis-reservas.page.html',
  styleUrls: ['./mis-reservas.page.scss']
})
export class MisReservasPage implements OnInit {
  reservas = signal<ReservaResponse[]>([]);
  cargando = signal(true);
  reservaSeleccionada = signal<ReservaResponse | null>(null);
  qrDataUrl = signal<string | null>(null);
  copiado = signal(false);

  constructor(
    private reservaService: ReservaService,
    private alertController: AlertController,
    private toastController: ToastController
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando.set(true);
    this.reservaService.misReservas().subscribe({
      next: (reservas) => {
        this.reservas.set(reservas);
        this.cargando.set(false);

        // Seleccionar la reserva activa más relevante (pendiente o confirmada)
        const activa = reservas.find(r => r.estado === 'confirmada' || r.estado === 'pendiente') || reservas[0];
        if (activa) {
          this.seleccionarReserva(activa);
        } else {
          this.reservaSeleccionada.set(null);
          this.qrDataUrl.set(null);
        }
      },
      error: () => {
        this.cargando.set(false);
      }
    });
  }

  seleccionarReserva(r: ReservaResponse): void {
    this.reservaSeleccionada.set(r);
    if (r.qrToken) {
      QRCode.toDataURL(r.qrToken, {
        width: 240,
        margin: 1,
        color: {
          dark: '#000000',
          light: '#FFFFFF'
        }
      })
        .then((url) => this.qrDataUrl.set(url))
        .catch(() => this.qrDataUrl.set(null));
    } else {
      this.qrDataUrl.set(null);
    }
  }

  async copiarToken(token: string | null): Promise<void> {
    if (!token) return;
    try {
      await navigator.clipboard.writeText(token);
      this.copiado.set(true);
      setTimeout(() => this.copiado.set(false), 2500);

      const toast = await this.toastController.create({
        message: 'Token copiado al portapapeles',
        duration: 2000,
        position: 'bottom',
        color: 'success'
      });
      await toast.present();
    } catch {
      this.copiado.set(true);
      setTimeout(() => this.copiado.set(false), 2000);
    }
  }

  colorBadgeEstado(estado: string): string {
    switch (estado) {
      case 'pendiente': return 'badge-amber';
      case 'confirmada': return 'badge-green';
      case 'completada': return 'badge-blue';
      case 'cancelada':
      case 'expirada': return 'badge-gray';
      default: return 'badge-gray';
    }
  }

  async confirmarCancelacion(reserva: ReservaResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Cancelar Reserva',
      subHeader: `${reserva.parqueaderoNombre} — Plaza ${reserva.plazaCodigo}`,
      message: '¿Estás seguro de que deseas cancelar tu reserva? La plaza será liberada inmediatamente.',
      buttons: [
        { text: 'Volver', role: 'cancel' },
        { text: 'Sí, cancelar reserva', role: 'destructive', handler: () => this.cancelar(reserva.id) }
      ]
    });
    await alert.present();
  }

  private cancelar(reservaId: number): void {
    this.reservaService.cancelar(reservaId).subscribe({
      next: () => this.cargar(),
      error: () => this.cargar()
    });
  }
}
