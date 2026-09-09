import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { IonicModule } from '@ionic/angular';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-registro',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IonicModule, RouterLink],
  templateUrl: './registro.page.html',
  styleUrls: ['./registro.page.scss']
})
export class RegistroPage {
  enviando = signal(false);
  errorMensaje = signal<string | null>(null);
  registroExitoso = signal(false);

  form = this.fb.group({
    nombre: ['', Validators.required],
    cedula: ['', Validators.required],
    correo: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
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

    this.enviando.set(true);
    this.errorMensaje.set(null);

    this.authService.registro(this.form.getRawValue() as any).subscribe({
      next: () => {
        this.enviando.set(false);
        this.registroExitoso.set(true);
      },
      error: (err) => {
        this.enviando.set(false);
        this.errorMensaje.set(err.error?.mensaje ?? 'No se pudo completar el registro.');
      }
    });
  }

  irALogin(): void {
    this.router.navigateByUrl('/login');
  }
}
