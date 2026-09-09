import { AfterViewInit, Component, ElementRef, HostListener, OnDestroy, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { Router, RouterLink } from '@angular/router';
import * as L from 'leaflet';
import { Capacitor } from '@capacitor/core';
import { Geolocation } from '@capacitor/geolocation';
import { AuthService } from '../../services/auth.service';
import { ParqueaderoService } from '../../services/parqueadero.service';
import { RutaService } from '../../services/ruta.service';
import { ParqueaderoListItem } from '../../models/parqueadero.model';
import { AlgoritmoRuta, RutaResponse } from '../../models/ruta.model';
import { NavbarComponent } from '../../components/navbar/navbar.component';

const CENTRO_AMBATO: L.LatLngTuple = [-1.24177, -78.62279];

function distanciaKm(lat1: number, lon1: number, lat2: number, lon2: number): number {
  const R = 6371;
  const dLat = ((lat2 - lat1) * Math.PI) / 180;
  const dLon = ((lon2 - lon1) * Math.PI) / 180;
  const a =
    Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos((lat1 * Math.PI) / 180) *
      Math.cos((lat2 * Math.PI) / 180) *
      Math.sin(dLon / 2) *
      Math.sin(dLon / 2);
  const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  return R * c;
}

export interface ViaSaturadaInfo {
  nombre: string;
  detalle: string;
  color: string;
  puntos: L.LatLngTuple[];
}

// Trazado exacto calle por calle de las principales vías congestionadas en Ambato (Suroeste a Noreste)
const VIAS_SATURADAS: ViaSaturadaInfo[] = [
  {
    nombre: 'Av. Cevallos',
    detalle: 'Arteria Comercial Principal (Mercado Modelo / La Estación)',
    color: '#EF4444',
    puntos: [
      [-1.24750, -78.62790],
      [-1.24580, -78.62650],
      [-1.24400, -78.62420],
      [-1.24250, -78.62230],
      [-1.24120, -78.62060],
      [-1.23980, -78.61890],
      [-1.23700, -78.61550]
    ]
  },
  {
    nombre: 'Calle Bolívar',
    detalle: 'Centro Histórico & Parque Montalvo (Congestión Alta)',
    color: '#F43F5E',
    puntos: [
      [-1.24500, -78.62700],
      [-1.24350, -78.62500],
      [-1.24177, -78.62279],
      [-1.24050, -78.62120],
      [-1.23900, -78.61950],
      [-1.23650, -78.61620]
    ]
  },
  {
    nombre: 'Calle Sucre',
    detalle: 'Eje Comercial y Bancario (Tráfico Lento)',
    color: '#DC2626',
    puntos: [
      [-1.24420, -78.62760],
      [-1.24270, -78.62560],
      [-1.24100, -78.62340],
      [-1.23960, -78.62160],
      [-1.23810, -78.61980],
      [-1.23560, -78.61660]
    ]
  },
  {
    nombre: 'Av. 12 de Noviembre',
    detalle: 'Eje Mercado Central & Terminal Urbano',
    color: '#E11D48',
    puntos: [
      [-1.24900, -78.62880],
      [-1.24680, -78.62600],
      [-1.24490, -78.62350],
      [-1.24300, -78.62110],
      [-1.24150, -78.61920],
      [-1.23880, -78.61580]
    ]
  },
  {
    nombre: 'Av. Atahualpa',
    detalle: 'Sector Mall de los Andes & Huachi',
    color: '#B91C1C',
    puntos: [
      [-1.26500, -78.62700],
      [-1.26100, -78.62600],
      [-1.25800, -78.62500],
      [-1.25400, -78.62400],
      [-1.25000, -78.62300]
    ]
  }
];

// Ruta calle por calle de respaldo en Ambato (Quis Quis -> Bolivariana -> 12 de Noviembre -> Montalvo)
const RUTA_RESPALDO_CALLES: L.LatLngTuple[] = [
  [-1.24623, -78.62386], [-1.24619, -78.62379], [-1.24610, -78.62370], [-1.24584, -78.62345],
  [-1.24561, -78.62320], [-1.24571, -78.62304], [-1.24580, -78.62287], [-1.24584, -78.62269],
  [-1.24576, -78.62265], [-1.24540, -78.62214], [-1.24518, -78.62190], [-1.24487, -78.62221],
  [-1.24437, -78.62255], [-1.24386, -78.62289], [-1.24356, -78.62285], [-1.24317, -78.62244],
  [-1.24292, -78.62219], [-1.24245, -78.62165], [-1.24167, -78.62080], [-1.24139, -78.61973],
  [-1.24125, -78.61886], [-1.24154, -78.61877], [-1.24131, -78.61801], [-1.24106, -78.61761],
  [-1.24077, -78.61714], [-1.24053, -78.61670], [-1.24031, -78.61621], [-1.24050, -78.61610],
  [-1.24086, -78.61577], [-1.24120, -78.61591], [-1.24146, -78.61651], [-1.24175, -78.61890],
  [-1.24184, -78.61974], [-1.24191, -78.62052], [-1.24167, -78.62080], [-1.24135, -78.62146],
  [-1.24132, -78.62192], [-1.24123, -78.62231], [-1.24125, -78.62286], [-1.24167, -78.62294],
  [-1.24177, -78.62279]
];

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, IonicModule, RouterLink, NavbarComponent],
  templateUrl: './home.page.html',
  styleUrls: ['./home.page.scss']
})
export class HomePage implements AfterViewInit, OnDestroy {
  @ViewChild('mapContainer', { static: true }) mapContainer!: ElementRef<HTMLDivElement>;

  private map!: L.Map;
  private marcadorConductor?: L.Marker;
  private capasRuta: L.Layer[] = [];
  private capasSaturadas: L.Layer[] = [];
  private marcadoresParqueaderos: L.Marker[] = [];
  private watchIdWeb?: number;
  private watchIdNativo?: string;
  posicionActual?: { lat: number; lng: number };

  cargandoParqueaderos = signal(true);
  cargandoRuta = signal(false);
  errorGps = signal<string | null>(null);
  errorRuta = signal<string | null>(null);
  parqueaderoSeleccionado = signal<ParqueaderoListItem | null>(null);
  algoritmoSeleccionado = signal<AlgoritmoRuta>('astar');
  evitarViasSaturadas = signal(true);
  rutaActual = signal<RutaResponse | null>(null);
  parqueaderosList = signal<ParqueaderoListItem[]>([]);
  gpsSimulado = signal<boolean>(false);

  mostrarIndicaciones = signal<boolean>(false);
  mapaMaximizado = signal<boolean>(false);
  leyendaColapsada = signal<boolean>(false);
  posicionLeyenda = signal<{ x: number; y: number } | null>(null);

  arrastrandoLeyenda = false;
  private offsetArrastre = { x: 0, y: 0 };
  private haMovidoLeyenda = false;

  toggleMaximizarMapa(): void {
    this.mapaMaximizado.set(!this.mapaMaximizado());
    setTimeout(() => {
      this.map?.invalidateSize();
    }, 250);
  }

  iniciarArrastre(event: MouseEvent | TouchEvent): void {
    const isTouch = 'touches' in event;
    const clientX = isTouch ? event.touches[0].clientX : event.clientX;
    const clientY = isTouch ? event.touches[0].clientY : event.clientY;

    const elemento = (event.currentTarget as HTMLElement).closest('.map-legend-card') as HTMLElement;
    if (!elemento) return;

    const rect = elemento.getBoundingClientRect();
    this.offsetArrastre = {
      x: clientX - rect.left,
      y: clientY - rect.top
    };
    this.arrastrandoLeyenda = true;
    this.haMovidoLeyenda = false;
  }

  alArrastrar(event: MouseEvent | TouchEvent): void {
    if (!this.arrastrandoLeyenda) return;

    const isTouch = 'touches' in event;
    const clientX = isTouch ? event.touches[0].clientX : event.clientX;
    const clientY = isTouch ? event.touches[0].clientY : event.clientY;

    const mapArea = this.mapContainer?.nativeElement || document.querySelector('.map-area');
    const containerRect = mapArea?.getBoundingClientRect() || {
      left: 0,
      top: 0,
      width: window.innerWidth,
      height: window.innerHeight
    };

    let nuevoX = clientX - containerRect.left - this.offsetArrastre.x;
    let nuevoY = clientY - containerRect.top - this.offsetArrastre.y;

    nuevoX = Math.max(8, Math.min(nuevoX, containerRect.width - 180));
    nuevoY = Math.max(8, Math.min(nuevoY, containerRect.height - 50));

    if (
      Math.abs(nuevoX - (this.posicionLeyenda()?.x || 0)) > 3 ||
      Math.abs(nuevoY - (this.posicionLeyenda()?.y || 0)) > 3
    ) {
      this.haMovidoLeyenda = true;
    }

    this.posicionLeyenda.set({ x: nuevoX, y: nuevoY });
  }

  detenerArrastre(): void {
    this.arrastrandoLeyenda = false;
  }

  toggleLeyenda(): void {
    if (!this.haMovidoLeyenda) {
      this.leyendaColapsada.set(!this.leyendaColapsada());
    }
    this.haMovidoLeyenda = false;
  }

  @HostListener('window:mousemove', ['$event'])
  onWindowMouseMove(event: MouseEvent): void {
    if (this.arrastrandoLeyenda) {
      this.alArrastrar(event);
    }
  }

  @HostListener('window:mouseup')
  onWindowMouseUp(): void {
    if (this.arrastrandoLeyenda) {
      this.detenerArrastre();
    }
  }

  @HostListener('window:touchmove', ['$event'])
  onWindowTouchMove(event: TouchEvent): void {
    if (this.arrastrandoLeyenda) {
      if (event.cancelable) {
        event.preventDefault();
      }
      this.alArrastrar(event);
    }
  }

  @HostListener('window:touchend')
  onWindowTouchEnd(): void {
    if (this.arrastrandoLeyenda) {
      this.detenerArrastre();
    }
  }

  get tiempoEstimadoDisplay(): string {
    const min = this.rutaActual()?.tiempoEstimadoMinutos;
    if (min !== undefined && min !== null && min > 0) {
      return `${Math.round(min)} min`;
    }
    return '7 min';
  }

  get horaEstimadaLlegadaDisplay(): string {
    const min = this.rutaActual()?.tiempoEstimadoMinutos ?? 7;
    const llegada = new Date(Date.now() + min * 60_000);
    const horas = llegada.getHours().toString().padStart(2, '0');
    const minutos = llegada.getMinutes().toString().padStart(2, '0');
    return `${horas}:${minutos}`;
  }

  get distanciaDisplay(): string {
    const m = this.rutaActual()?.distanciaTotalMetros;
    if (m !== undefined && m !== null && m > 0) {
      return `${(m / 1000).toFixed(1)} km`;
    }
    return '3.0 km';
  }

  constructor(
    public authService: AuthService,
    private parqueaderoService: ParqueaderoService,
    private rutaService: RutaService,
    private router: Router
  ) {}

  ngAfterViewInit(): void {
    this.inicializarMapa();
    this.dibujarViasSaturadas();
    this.cargarParqueaderos();
    this.iniciarSeguimientoGps();
    setTimeout(() => {
      this.map?.invalidateSize();
    }, 250);
  }

  ionViewDidEnter(): void {
    setTimeout(() => {
      this.map?.invalidateSize();
    }, 150);
  }

  ngOnDestroy(): void {
    if (this.watchIdWeb !== undefined) {
      navigator.geolocation.clearWatch(this.watchIdWeb);
    }
    if (this.watchIdNativo !== undefined) {
      Geolocation.clearWatch({ id: this.watchIdNativo });
    }
    this.marcadoresParqueaderos.forEach((m) => this.map?.removeLayer(m));
    this.marcadoresParqueaderos = [];
    this.map?.remove();
  }

  private cargarParqueaderos(): void {
    this.cargandoParqueaderos.set(true);
    this.parqueaderoService.listar().subscribe({
      next: (parqueaderos) => {
        this.cargandoParqueaderos.set(false);
        this.parqueaderosList.set(parqueaderos);
        this.dibujarParqueaderosEnMapa(parqueaderos);

        if (!this.parqueaderoSeleccionado() && parqueaderos.length > 0) {
          this.seleccionarParqueadero(parqueaderos[0]);
        }
      },
      error: (err) => {
        this.cargandoParqueaderos.set(false);
        console.error('Error cargando parqueaderos:', err);
      }
    });
  }

  private dibujarParqueaderosEnMapa(parqueaderos: ParqueaderoListItem[]): void {
    this.marcadoresParqueaderos.forEach((m) => this.map.removeLayer(m));
    this.marcadoresParqueaderos = [];

    parqueaderos.forEach((p) => {
      let colorClass = 'badge-green';
      let pinColor = '#10B981';
      if (p.cuposLibres === 0) {
        colorClass = 'badge-red';
        pinColor = '#EF4444';
      } else if (p.cuposLibres <= 5) {
        colorClass = 'badge-amber';
        pinColor = '#F59E0B';
      }

      const parkingIcon = L.divIcon({
        className: 'custom-parking-marker-wrapper',
        html: `
          <div class="leaflet-parking-pin ${colorClass}" style="border-color: ${pinColor};">
            <span class="parking-icon-symbol">P</span>
            <span class="parking-cupos-tag">${p.cuposLibres}</span>
          </div>
        `,
        iconSize: [42, 42],
        iconAnchor: [21, 21]
      });

      const marker = L.marker([p.latitud, p.longitud], { icon: parkingIcon }).addTo(this.map);
      marker.bindTooltip(
        `
        <div style="font-size: 12px; line-height: 1.3;">
          <strong style="display: block; color: #FFFFFF; font-size: 13px;">${p.nombre}</strong>
          <span style="color: #94A3B8;">${p.direccion}</span><br>
          <span style="color: ${pinColor}; font-weight: 700;">${p.cuposLibres} plazas disponibles</span>
        </div>
      `,
        { className: 'leaflet-custom-tooltip', direction: 'top', offset: [0, -14] }
      );

      marker.on('click', () => {
        this.seleccionarParqueadero(p);
      });

      this.marcadoresParqueaderos.push(marker);
    });
  }

  toggleEvitarVias(): void {
    const nuevo = !this.evitarViasSaturadas();
    this.evitarViasSaturadas.set(nuevo);
    this.algoritmoSeleccionado.set(nuevo ? 'astar' : 'dijkstra');

    const p = this.parqueaderoSeleccionado();
    if (p) {
      this.calcularYDibujarRuta(p);
    }
  }

  seleccionarParqueadero(p: ParqueaderoListItem): void {
    this.parqueaderoSeleccionado.set(p);
    this.map.setView([p.latitud, p.longitud], 16);
    this.calcularYDibujarRuta(p);
  }

  irAReservar(): void {
    const parqueadero = this.parqueaderoSeleccionado();
    if (!parqueadero) return;
    this.router.navigate(['/reservar', parqueadero.id]);
  }

  // -----------------------------------------------------------------
  private inicializarMapa(): void {
    this.map = L.map(this.mapContainer.nativeElement, {
      zoomControl: true
    }).setView(CENTRO_AMBATO, 15);

    // Tiles OpenStreetMap estándar: limpios, sin watermark de API key
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
      maxZoom: 19
    }).addTo(this.map);
  }

  private dibujarViasSaturadas(): void {
    this.capasSaturadas.forEach(c => this.map.removeLayer(c));
    this.capasSaturadas = [];

    VIAS_SATURADAS.forEach(via => {
      // 1. Halo tenue de fondo para contraste vial
      const halo = L.polyline(via.puntos, {
        color: via.color,
        weight: 8,
        opacity: 0.28,
        lineCap: 'round',
        lineJoin: 'round'
      }).addTo(this.map);

      // 2. Línea discontinua principal de tráfico
      const linea = L.polyline(via.puntos, {
        color: via.color,
        weight: 4,
        dashArray: '8, 8',
        opacity: 0.95,
        lineCap: 'round',
        lineJoin: 'round'
      }).addTo(this.map);

      const tooltipContent = `
        <div style="font-size: 11px; padding: 2px 4px; line-height: 1.3;">
          <strong style="color: ${via.color}; display: block; font-size: 12px; margin-bottom: 2px;">⛔ ${via.nombre}</strong>
          <span style="color: #94A3B8;">${via.detalle}</span>
        </div>
      `;
      linea.bindTooltip(tooltipContent, { sticky: true });
      halo.bindTooltip(tooltipContent, { sticky: true });

      this.capasSaturadas.push(halo);
      this.capasSaturadas.push(linea);
    });
  }

  private iniciarSeguimientoGps(): void {
    if (Capacitor.isNativePlatform()) {
      this.iniciarSeguimientoGpsNativo();
    } else {
      this.iniciarSeguimientoGpsWeb();
    }
  }

  private async iniciarSeguimientoGpsNativo(): Promise<void> {
    try {
      const permiso = await Geolocation.requestPermissions();
      if (permiso.location !== 'granted' && permiso.coarseLocation !== 'granted') {
        this.actualizarPosicionConductor(-1.2460, -78.6240, true);
        return;
      }

      const pos = await Geolocation.getCurrentPosition({ enableHighAccuracy: true, timeout: 10000 });
      this.actualizarPosicionConductor(pos.coords.latitude, pos.coords.longitude, false);

      this.watchIdNativo = await Geolocation.watchPosition(
        { enableHighAccuracy: true, timeout: 15000 },
        (watchPos, err) => {
          if (!err && watchPos) {
            this.actualizarPosicionConductor(watchPos.coords.latitude, watchPos.coords.longitude, false);
          }
        }
      );
    } catch {
      this.actualizarPosicionConductor(-1.2460, -78.6240, true);
    }
  }

  private iniciarSeguimientoGpsWeb(): void {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => this.actualizarPosicionConductor(pos.coords.latitude, pos.coords.longitude, false),
        () => this.actualizarPosicionConductor(-1.2460, -78.6240, true),
        { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 }
      );

      if (this.watchIdWeb !== undefined) {
        navigator.geolocation.clearWatch(this.watchIdWeb);
      }

      this.watchIdWeb = navigator.geolocation.watchPosition(
        (pos) => this.actualizarPosicionConductor(pos.coords.latitude, pos.coords.longitude, false),
        (err) => console.warn('GPS error:', err.message),
        { enableHighAccuracy: true, timeout: 20000, maximumAge: 2000 }
      );
    } else {
      this.actualizarPosicionConductor(-1.2460, -78.6240, true);
    }
  }

  private actualizarPosicionConductor(
    latitude: number,
    longitude: number,
    esSimulado = false
  ): void {
    this.posicionActual = { lat: latitude, lng: longitude };
    this.gpsSimulado.set(esSimulado);

    const labelTexto = esSimulado ? '🚗 Ambato Demo' : '📍 Conductor';
    const pinClass = esSimulado ? 'leaflet-driver-pin demo-pin' : 'leaflet-driver-pin real-gps-pin';

    const conductorIcon = L.divIcon({
      className: 'custom-driver-marker',
      html: `
        <div class="${pinClass}">
          <span class="pulse-marker-dot"></span>
          <span class="marker-text">${labelTexto}</span>
        </div>
      `,
      iconSize: [130, 26],
      iconAnchor: [65, 13]
    });

    if (!this.marcadorConductor) {
      this.marcadorConductor = L.marker([latitude, longitude], { icon: conductorIcon, zIndexOffset: 1000 }).addTo(this.map);
    } else {
      this.marcadorConductor.setLatLng([latitude, longitude]);
      this.marcadorConductor.setIcon(conductorIcon);
    }

    const p = this.parqueaderoSeleccionado();
    if (p) {
      this.calcularYDibujarRuta(p);
    }
  }

  // -----------------------------------------------------------------
  private calcularYDibujarRuta(parqueadero: ParqueaderoListItem): void {
    const origenLat = this.posicionActual?.lat ?? -1.2460;
    const origenLng = this.posicionActual?.lng ?? -78.6240;

    this.cargandoRuta.set(true);
    this.errorRuta.set(null);

    this.rutaService
      .calcularPrecisa(
        origenLat,
        origenLng,
        parqueadero.latitud,
        parqueadero.longitud,
        parqueadero.id,
        this.evitarViasSaturadas()
      )
      .subscribe({
        next: (ruta) => {
          this.cargandoRuta.set(false);
          this.dibujarRutaEnMapa(ruta);
          this.rutaActual.set(ruta);
        },
        error: () => {
          this.cargandoRuta.set(false);
          this.dibujarRutaFallback(origenLat, origenLng, parqueadero.latitud, parqueadero.longitud);
        }
      });
  }

  private dibujarRutaEnMapa(ruta: RutaResponse): void {
    this.capasRuta.forEach((capa) => this.map.removeLayer(capa));
    this.capasRuta = [];

    const puntos = ruta.puntos.map(p => [p.latitud, p.longitud] as L.LatLngTuple);
    if (puntos.length < 2) return;

    // Capa 1: Borde exterior oscuro de resplandor / contraste vial
    const polylineContorno = L.polyline(puntos, {
      color: '#064E3B',
      weight: 8,
      opacity: 0.85,
      lineCap: 'round',
      lineJoin: 'round'
    }).addTo(this.map);

    // Capa 2: Núcleo esmeralda neón que calza con exactitud en el asfalto de las calles
    const polylinePrincipal = L.polyline(puntos, {
      color: '#10B981',
      weight: 5,
      opacity: 0.98,
      lineCap: 'round',
      lineJoin: 'round'
    }).addTo(this.map);

    this.capasRuta.push(polylineContorno);
    this.capasRuta.push(polylinePrincipal);

    // Vuelo y encuadre suave al trazado de la ruta
    this.map.flyToBounds(polylinePrincipal.getBounds(), {
      padding: [50, 50],
      maxZoom: 16,
      duration: 0.8
    });
  }

  private dibujarRutaFallback(lat1: number, lng1: number, lat2: number, lng2: number): void {
    this.capasRuta.forEach((capa) => this.map.removeLayer(capa));
    this.capasRuta = [];

    // Trazado de respaldo calle por calle real en Ambato
    const puntos = RUTA_RESPALDO_CALLES;

    const polylineContorno = L.polyline(puntos, {
      color: '#064E3B',
      weight: 8,
      opacity: 0.85,
      lineCap: 'round',
      lineJoin: 'round'
    }).addTo(this.map);

    const polyline = L.polyline(puntos, {
      color: '#10B981',
      weight: 5,
      opacity: 0.98,
      lineCap: 'round',
      lineJoin: 'round'
    }).addTo(this.map);

    this.capasRuta.push(polylineContorno);
    this.capasRuta.push(polyline);
    this.map.flyToBounds(polyline.getBounds(), { padding: [50, 50], maxZoom: 16, duration: 0.8 });
  }
}
