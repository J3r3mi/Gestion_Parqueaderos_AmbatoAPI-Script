import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { IonicModule } from '@ionic/angular';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, IonicModule, RouterLink, RouterLinkActive],
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {
  constructor(
    public authService: AuthService,
    private router: Router
  ) {}

  salir(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  get rol(): string {
    return (this.authService.usuarioActual()?.rol || 'administrador').toLowerCase();
  }

  get isConductor(): boolean {
    return this.rol === 'conductor';
  }

  get isOperador(): boolean {
    return this.rol === 'operador';
  }

  get isAdmin(): boolean {
    return this.rol === 'administrador';
  }

  get rolTexto(): string {
    const rol = this.authService.usuarioActual()?.rol;
    if (!rol) return 'Administrador';
    return rol.charAt(0).toUpperCase() + rol.slice(1);
  }

  get nombreUsuario(): string {
    return this.authService.usuarioActual()?.nombre || 'Ing. Paulina Torres';
  }

  get cedulaUsuario(): string {
    return this.authService.usuarioActual()?.cedula || '1801234567';
  }
}
