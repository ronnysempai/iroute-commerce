import { Routes } from '@angular/router';

import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./features/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      {
        path: 'carga',
        loadComponent: () =>
          import('./features/upload/upload.component').then((m) => m.UploadComponent)
      },
      {
        path: 'proceso',
        loadComponent: () =>
          import('./features/process/process.component').then((m) => m.ProcessComponent)
      },
      {
        path: 'cuarentena',
        loadComponent: () =>
          import('./features/quarantine/quarantine.component').then((m) => m.QuarantineComponent)
      },
      { path: '', redirectTo: 'carga', pathMatch: 'full' }
    ]
  },
  { path: '**', redirectTo: '' }
];
