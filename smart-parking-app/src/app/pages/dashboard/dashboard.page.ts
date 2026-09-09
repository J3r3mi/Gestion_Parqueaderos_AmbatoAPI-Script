import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { DashboardCompleto, DashboardService } from '../../services/dashboard.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';

const INTERVALO_REFRESCO_MS = 30_000;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, IonicModule, RouterLink, NavbarComponent],
  templateUrl: './dashboard.page.html',
  styleUrls: ['./dashboard.page.scss']
})
export class DashboardPage implements OnInit, OnDestroy {
  datos = signal<DashboardCompleto | null>(null);
  cargando = signal(true);
  ultimaActualizacion = signal<Date | null>(null);

  private intervalId?: ReturnType<typeof setInterval>;

  constructor(
    public authService: AuthService,
    private dashboardService: DashboardService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargar();
    this.intervalId = setInterval(() => this.cargar(true), INTERVALO_REFRESCO_MS);
  }

  ngOnDestroy(): void {
    if (this.intervalId) clearInterval(this.intervalId);
  }

  refrescarManual(): void {
    this.cargar();
  }

  // Cálculos para el Gauge Radial de Ocupación
  porcentajeOcupacion(): number {
    const tot = this.plazasTotales();
    if (tot === 0) return 0;
    return Math.round((this.plazasOcupadas() / tot) * 100);
  }

  plazasTotales(): number {
    const val = this.datos()?.kpis?.plazasTotales;
    return val !== undefined && val !== null ? val : 0;
  }

  plazasOcupadas(): number {
    const val = this.datos()?.kpis?.plazasOcupadas;
    return val !== undefined && val !== null ? val : 0;
  }

  cuposLibres(): number {
    const val = this.datos()?.kpis?.cuposLibres;
    return val !== undefined && val !== null ? val : 0;
  }

  reservasActivas(): number {
    const val = this.datos()?.kpis?.reservasPendientesHoy;
    return val !== undefined && val !== null ? val : 0;
  }

  recaudacionTotal(): number {
    const val = this.datos()?.recaudacion?.total;
    return val !== undefined && val !== null ? val : 0;
  }

  totalParqueaderos(): number {
    return this.datos()?.kpisPorParqueadero?.length || (this.plazasTotales() > 0 ? 1 : 0);
  }

  gaugeOffset(): number {
    const r = 38;
    const c = 2 * Math.PI * r;
    const p = this.porcentajeOcupacion();
    return c - (p / 100) * c;
  }

  // Horarios de 07h a 21h calculados dinámicamente según accesos reales
  get horasChart() {
    const horasApi = this.datos()?.ocupacionHoraria || [];
    const rangoHoras = [7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21];

    let maxFlujo = 0;
    let horaPico = -1;

    rangoHoras.forEach(h => {
      const match = horasApi.find(item => item.hora === h);
      const total = (match?.entradas || 0) + (match?.salidas || 0);
      if (total > maxFlujo) {
        maxFlujo = total;
        horaPico = h;
      }
    });

    return rangoHoras.map(h => {
      const match = horasApi.find(item => item.hora === h);
      const horaStr = `${h.toString().padStart(2, '0')}h`;
      return {
        hora: horaStr,
        entradas: match ? match.entradas : 0,
        salidas: match ? match.salidas : 0,
        pico: maxFlujo > 0 && h === horaPico
      };
    });
  }

  alturaBarra(valor: number): string {
    if (valor <= 0) return '6px';
    const maxVal = Math.max(
      ...(this.datos()?.ocupacionHoraria || []).map(h => Math.max(h.entradas, h.salidas)),
      10
    );
    const porcentaje = Math.min(100, Math.max(12, Math.round((valor / maxVal) * 100)));
    return `${porcentaje}%`;
  }

  private cargar(esRefrescoSilencioso = false): void {
    if (!esRefrescoSilencioso) this.cargando.set(true);

    this.dashboardService.cargarTodo().subscribe({
      next: (datos) => {
        this.datos.set(datos);
        this.cargando.set(false);
        this.ultimaActualizacion.set(new Date());
      },
      error: () => this.cargando.set(false)
    });
  }
}
