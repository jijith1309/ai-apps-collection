import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { ShellComponent } from './shared/components/layout/shell.component';
import { LandingComponent } from './features/landing/pages/landing.component';

export const routes: Routes = [
  {
    path: '',
    component: LandingComponent,
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/pages/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'documents',
        loadComponent: () => import('./features/documents/pages/documents.component').then(m => m.DocumentsComponent)
      },
      {
        path: 'chat',
        loadComponent: () => import('./features/chat/pages/chat.component').then(m => m.ChatComponent)
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
