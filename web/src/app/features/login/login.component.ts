import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';

/** Pantalla de inicio de sesion (login opcional). */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  username = 'admin';
  password = '';
  error = '';
  loading = false;
  showPassword = false;

  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  submit(): void {
    if (!this.username.trim() || !this.password) {
      this.error = 'Ingresa usuario y contraseña.';
      return;
    }

    this.loading = true;
    this.error = '';

    this.auth.login({ username: this.username.trim(), password: this.password }).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/carga']);
      },
      error: (error) => {
        this.loading = false;
        this.error = error?.error?.message ?? 'Usuario o contraseña incorrectos.';
      }
    });
  }
}
