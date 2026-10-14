import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../core/services/auth.service';

/** Layout de la aplicacion: barra de navegacion + area de contenido. */
@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.component.html',
  styleUrl: './shell.component.css'
})
export class ShellComponent {
  constructor(
    private readonly auth: AuthService,
    private readonly router: Router
  ) {}

  get username(): string {
    return this.auth.username;
  }

  get role(): string {
    return this.auth.role;
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
