import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

type PasoRecuperacion = 'solicitar' | 'restablecer' | 'exito';

@Component({
  selector: 'app-recuperar-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IonicModule, RouterLink],
  templateUrl: './recuperar-password.page.html',
  styleUrls: ['./recuperar-password.page.scss']
})
export class RecuperarPasswordPage {
  paso = signal<PasoRecuperacion>('solicitar');
  enviando = signal(false);
  errorMensaje = signal<string | null>(null);
  mensajeInfo = signal<string | null>(null);

  formSolicitar = this.fb.group({
    identificador: ['', Validators.required]
  });

  formRestablecer = this.fb.group({
    token: ['', Validators.required],
    nuevaPassword: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  onSolicitar(): void {
    if (this.formSolicitar.invalid) {
      this.formSolicitar.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.errorMensaje.set(null);

    const { identificador } = this.formSolicitar.getRawValue();

    this.authService.solicitarRecuperacion(identificador!).subscribe({
      next: (respuesta) => {
        this.enviando.set(false);
        this.mensajeInfo.set(respuesta.mensaje);

        // MODO DEMO: como no hay servicio de correo real configurado, el backend
        // devuelve el token directamente. En producción esto NO pasaría —
        // el usuario lo recibiría por correo y llegaría aquí con un link.
        if (respuesta.tokenSoloParaDemo) {
          this.formRestablecer.patchValue({ token: respuesta.tokenSoloParaDemo });
        }

        this.paso.set('restablecer');
      },
      error: () => {
        this.enviando.set(false);
        this.errorMensaje.set('No se pudo procesar la solicitud. Intenta de nuevo.');
      }
    });
  }

  onRestablecer(): void {
    if (this.formRestablecer.invalid) {
      this.formRestablecer.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.errorMensaje.set(null);

    const { token, nuevaPassword } = this.formRestablecer.getRawValue();

    this.authService.restablecerPassword(token!, nuevaPassword!).subscribe({
      next: () => {
        this.enviando.set(false);
        this.paso.set('exito');
      },
      error: (err) => {
        this.enviando.set(false);
        this.errorMensaje.set(err.error?.mensaje ?? 'No se pudo restablecer la contraseña.');
      }
    });
  }

  irALogin(): void {
    this.router.navigateByUrl('/login');
  }
}
