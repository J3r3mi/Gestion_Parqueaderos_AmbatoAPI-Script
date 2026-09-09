import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { IonicModule, ToastController } from '@ionic/angular';
import { ActivatedRoute, RouterLink } from '@angular/router';
import * as QRCode from 'qrcode';

import { NavbarComponent } from '../../components/navbar/navbar.component';
import { ParqueaderoService } from '../../services/parqueadero.service';
import { ReservaService } from '../../services/reserva.service';
import { Plaza, Tarifa } from '../../models/parqueadero.model';
import { ReservaResponse } from '../../models/reserva.model';

@Component({
  selector: 'app-reservar',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IonicModule, RouterLink, NavbarComponent],
  templateUrl: './reservar.page.html',
  styleUrls: ['./reservar.page.scss']
})
export class ReservarPage implements OnInit {
  parqueaderoId!: number;

  cargando = signal(true);
  enviando = signal(false);
  errorMensaje = signal<string | null>(null);
  copiado = signal(false);

  parqueaderoNombre = signal<string>('Parqueadero Ambato');
  parqueaderoDireccion = signal<string>('Centro Histórico, Ambato');

  plazasLibres = signal<Plaza[]>([]);
  tarifas = signal<Tarifa[]>([]);

  reservaConfirmada = signal<ReservaResponse | null>(null);
  qrDataUrl = signal<string | null>(null);

  horaMinima = new Date().toISOString();

  // Gestión de 24 Horas para Arribo
  horas24 = Array.from({ length: 24 }, (_, i) => i);
  minutosOpciones = [0, 15, 30, 45];

  fechaHoy = this.formatearFechaInput(new Date());
  fechaManana = this.formatearFechaInput(new Date(Date.now() + 86400000));

  fechaSeleccionada = signal<string>(this.formatearFechaInput(new Date()));
  horaSeleccionada = signal<number>(new Date().getHours());
  minutoSeleccionado = signal<number>(Math.min(45, Math.floor(new Date().getMinutes() / 15) * 15 + 15) % 60);

  form = this.fb.group({
    plazaId: [null as number | null, [Validators.required]],
    horaEstimadaArribo: [this.horaMinima, [Validators.required]]
  });

  constructor(
    private route: ActivatedRoute,
    private fb: FormBuilder,
    private parqueaderoService: ParqueaderoService,
    private reservaService: ReservaService,
    private toastController: ToastController
  ) {}

  ngOnInit(): void {
    this.parqueaderoId = Number(this.route.snapshot.paramMap.get('id')) || 1;
    this.sincronizarHoraArribo();
    this.cargarDatos();
  }

  private formatearFechaInput(d: Date): string {
    const y = d.getFullYear();
    const m = (d.getMonth() + 1).toString().padStart(2, '0');
    const day = d.getDate().toString().padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  seleccionarFechaRapida(tipo: 'hoy' | 'manana'): void {
    const d = new Date();
    if (tipo === 'manana') {
      d.setDate(d.getDate() + 1);
    }
    this.fechaSeleccionada.set(this.formatearFechaInput(d));
    this.sincronizarHoraArribo();
  }

  cambiarFechaManual(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    if (val) {
      this.fechaSeleccionada.set(val);
      this.sincronizarHoraArribo();
    }
  }

  seleccionarHora(h: number): void {
    this.horaSeleccionada.set(h);
    this.sincronizarHoraArribo();
  }

  seleccionarMinuto(m: number): void {
    this.minutoSeleccionado.set(m);
    this.sincronizarHoraArribo();
  }

  cambiarHoraInputDirecto(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    if (val && val.includes(':')) {
      const [h, m] = val.split(':').map(Number);
      this.horaSeleccionada.set(h);
      this.minutoSeleccionado.set(m);
      this.sincronizarHoraArribo();
    }
  }

  get valorInputTime24(): string {
    const h = this.horaSeleccionada().toString().padStart(2, '0');
    const m = this.minutoSeleccionado().toString().padStart(2, '0');
    return `${h}:${m}`;
  }

  private sincronizarHoraArribo(): void {
    const [year, month, day] = this.fechaSeleccionada().split('-').map(Number);
    const date = new Date(year, month - 1, day, this.horaSeleccionada(), this.minutoSeleccionado(), 0);
    this.form.patchValue({ horaEstimadaArribo: date.toISOString() });
  }

  get horaEstimadaDisplay(): string {
    const h = this.horaSeleccionada().toString().padStart(2, '0');
    const m = this.minutoSeleccionado().toString().padStart(2, '0');
    const esHoy = this.fechaSeleccionada() === this.formatearFechaInput(new Date());
    const prefijo = esHoy ? 'Hoy' : this.fechaSeleccionada();
    return `${prefijo} a las ${h}:${m} (${h}h${m})`;
  }

  seleccionarPlaza(id: number): void {
    this.form.patchValue({ plazaId: id });
  }

  tarifaSeleccionada(): Tarifa | undefined {
    const plazaId = this.form.get('plazaId')?.value;
    const plaza = this.plazasLibres().find((p) => p.id === plazaId);
    if (!plaza) return undefined;
    return this.tarifas().find((t) => t.tipoVehiculo === plaza.tipoVehiculo);
  }

  async copiarToken(token: string | null): Promise<void> {
    if (!token) return;
    try {
      await navigator.clipboard.writeText(token);
      this.copiado.set(true);
      setTimeout(() => this.copiado.set(false), 2000);

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

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.errorMensaje.set(null);

    const { plazaId, horaEstimadaArribo } = this.form.getRawValue();

    this.reservaService
      .crear({ plazaId: plazaId!, horaEstimadaArribo: horaEstimadaArribo! })
      .subscribe({
        next: (reserva) => {
          this.enviando.set(false);
          this.reservaConfirmada.set(reserva);
          this.generarImagenQr(reserva.qrToken);
        },
        error: (err) => {
          this.enviando.set(false);
          if (err.status === 409) {
            this.errorMensaje.set(
              err.error?.mensaje ?? 'Esa plaza ya no está disponible. Por favor elige otra plaza.'
            );
            this.cargarDatos();
          } else {
            this.errorMensaje.set(
              err.error?.mensaje ?? 'No se pudo registrar la reserva. Intenta con otra plaza o vuelve a intentarlo.'
            );
          }
        }
      });
  }

  private cargarDatos(): void {
    this.cargando.set(true);

    // Obtener datos del parqueadero para el header
    this.parqueaderoService.listar().subscribe({
      next: (parqueaderos) => {
        const p = parqueaderos.find(x => x.id === this.parqueaderoId);
        if (p) {
          this.parqueaderoNombre.set(p.nombre);
          this.parqueaderoDireccion.set(p.direccion);
        } else {
          this.asignarNombrePorDefecto();
        }
      },
      error: () => this.asignarNombrePorDefecto()
    });

    this.parqueaderoService.listarPlazas(this.parqueaderoId).subscribe({
      next: (plazas) => {
        const libres = plazas.filter((p) => p.estado === 'libre');
        if (libres.length > 0) {
          this.plazasLibres.set(libres);
          this.form.patchValue({ plazaId: libres[0].id });
        } else {
          this.cargarPlazasFallback();
        }
        this.cargando.set(false);
      },
      error: () => {
        this.cargarPlazasFallback();
        this.cargando.set(false);
      }
    });

    this.parqueaderoService.listarTarifas(this.parqueaderoId).subscribe({
      next: (tarifas) => {
        if (tarifas.length > 0) {
          this.tarifas.set(tarifas);
        } else {
          this.tarifas.set([{ id: 1, tipoVehiculo: 'auto', valorHora: 0.85, vigenteDesde: new Date().toISOString() }]);
        }
      },
      error: () => {
        this.tarifas.set([{ id: 1, tipoVehiculo: 'auto', valorHora: 0.85, vigenteDesde: new Date().toISOString() }]);
      }
    });
  }

  private asignarNombrePorDefecto(): void {
    const nombres: Record<number, { nombre: string; direccion: string }> = {
      1: { nombre: 'Parqueadero Parque Montalvo', direccion: 'Calle Bolívar y Castillo (Pleno Centro Histórico)' },
      2: { nombre: 'Parqueadero Mercado Central', direccion: 'Calle 13 de Abril y Cevallos' },
      3: { nombre: 'Parqueadero Centro Ambato', direccion: 'Avenida Cevallos y Martínez' },
      4: { nombre: 'Parqueadero La Merced Plaza', direccion: 'Calle Bolívar y Rocafuerte' },
      5: { nombre: 'Parqueadero Terminal Terrestre', direccion: 'Av. Las Américas y Av. Colombia' },
      6: { nombre: 'Parqueadero Ficoa Paseo', direccion: 'Av. Los Guaytambos y Delicias' }
    };
    const info = nombres[this.parqueaderoId] || { nombre: 'Parqueadero Ambato Centro', direccion: 'Av. Cevallos' };
    this.parqueaderoNombre.set(info.nombre);
    this.parqueaderoDireccion.set(info.direccion);
  }

  private cargarPlazasFallback(): void {
    const fallbackPlazas: Plaza[] = [
      { id: 1, codigo: 'A-01', tipoVehiculo: 'auto', estado: 'libre' },
      { id: 2, codigo: 'A-02', tipoVehiculo: 'auto', estado: 'libre' },
      { id: 3, codigo: 'A-03', tipoVehiculo: 'auto', estado: 'libre' },
      { id: 4, codigo: 'A-04', tipoVehiculo: 'auto', estado: 'libre' },
      { id: 5, codigo: 'M-01', tipoVehiculo: 'moto', estado: 'libre' },
      { id: 6, codigo: 'M-02', tipoVehiculo: 'moto', estado: 'libre' }
    ];
    this.plazasLibres.set(fallbackPlazas);
    this.form.patchValue({ plazaId: fallbackPlazas[0].id });
  }

  private generarImagenQr(token: string | null): void {
    if (!token) return;
    QRCode.toDataURL(token, {
      width: 240,
      margin: 1,
      color: {
        dark: '#000000',
        light: '#FFFFFF'
      }
    })
      .then((url) => this.qrDataUrl.set(url))
      .catch(() => this.qrDataUrl.set(null));
  }
}
