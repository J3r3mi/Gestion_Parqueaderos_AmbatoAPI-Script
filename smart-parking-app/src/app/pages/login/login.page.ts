import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { IonicModule } from '@ionic/angular';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IonicModule, RouterLink],
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss']
})
export class LoginPage {
  cargando = signal(false);
  errorMensaje = signal<string | null>(null);

  form = this.fb.group({
    identificador: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.cargando.set(true);
    this.errorMensaje.set(null);

    const { identificador, password } = this.form.getRawValue();

    this.authService
      .login({ identificador: identificador!, password: password! })
      .subscribe({
        next: (respuesta) => {
          this.cargando.set(false);
          // Redirección según rol: el mismo login sirve para Conductor, Administrador y Operador.
          if (respuesta.usuario.rol === 'administrador') {
            this.router.navigateByUrl('/dashboard');
          } else if (respuesta.usuario.rol === 'operador') {
            this.router.navigateByUrl('/validar-acceso');
          } else {
            this.router.navigateByUrl('/home');
          }
        },
        error: (err) => {
          this.cargando.set(false);
          this.errorMensaje.set(
            err.status === 401
              ? 'Correo/cédula o contraseña incorrectos.'
              : 'No se pudo conectar con el servidor. Verifica que el backend esté corriendo.'
          );
        }
      });
  }
}
