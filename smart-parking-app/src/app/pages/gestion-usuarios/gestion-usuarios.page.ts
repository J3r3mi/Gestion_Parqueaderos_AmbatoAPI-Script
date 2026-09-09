import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { IonicModule, AlertController } from '@ionic/angular';
import { UsuarioService } from '../../services/usuario.service';
import { RolStaff, UsuarioListItem } from '../../models/usuario.model';

@Component({
  selector: 'app-gestion-usuarios',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, IonicModule],
  templateUrl: './gestion-usuarios.page.html',
  styleUrls: ['./gestion-usuarios.page.scss']
})
export class GestionUsuariosPage implements OnInit {
  usuarios = signal<UsuarioListItem[]>([]);
  cargando = signal(true);
  mostrandoFormulario = signal(false);
  enviando = signal(false);
  errorMensaje = signal<string | null>(null);

  form = this.fb.group({
    nombre: ['', Validators.required],
    cedula: ['', Validators.required],
    correo: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rol: ['operador' as RolStaff, Validators.required]
  });

  constructor(
    private fb: FormBuilder,
    private usuarioService: UsuarioService,
    private alertController: AlertController
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  cargar(): void {
    this.cargando.set(true);
    this.usuarioService.listarStaff().subscribe({
      next: (usuarios) => {
        this.usuarios.set(usuarios);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  toggleFormulario(): void {
    this.mostrandoFormulario.set(!this.mostrandoFormulario());
    this.errorMensaje.set(null);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.enviando.set(true);
    this.errorMensaje.set(null);

    this.usuarioService.registrarStaff(this.form.getRawValue() as any).subscribe({
      next: () => {
        this.enviando.set(false);
        this.mostrandoFormulario.set(false);
        this.form.reset({ rol: 'operador' });
        this.cargar();
      },
      error: (err) => {
        this.enviando.set(false);
        this.errorMensaje.set(err.error?.mensaje ?? 'No se pudo registrar el usuario.');
      }
    });
  }

  async confirmarCambioEstado(usuario: UsuarioListItem): Promise<void> {
    const accion = usuario.activo ? 'desactivar' : 'activar';
    const alert = await this.alertController.create({
      header: `${accion === 'activar' ? 'Activar' : 'Desactivar'} usuario`,
      message: `¿Seguro que quieres ${accion} a ${usuario.nombre}?`,
      buttons: [
        { text: 'Cancelar', role: 'cancel' },
        {
          text: 'Confirmar',
          handler: () => this.cambiarEstado(usuario.id, !usuario.activo)
        }
      ]
    });
    await alert.present();
  }

  private cambiarEstado(usuarioId: number, activo: boolean): void {
    this.usuarioService.actualizarEstado(usuarioId, activo).subscribe({
      next: () => this.cargar(),
      error: async (err) => {
        const alert = await this.alertController.create({
          header: 'No se pudo actualizar',
          message: err.error?.mensaje ?? 'Intenta de nuevo.',
          buttons: ['OK']
        });
        await alert.present();
      }
    });
  }
}
