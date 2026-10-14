import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth-guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login').then(m => m.Login),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/layout/app-shell/app-shell').then(m => m.AppShell),
    children: [
      {
    path: '',
    loadComponent: () =>
      import('./features/tickets/ticket-list/ticket-list.component')
        .then(m => m.TicketListComponent),
      },
      {
        path: 'tickets/new',
        loadComponent: () =>
          import('./features/tickets/create-ticket/create-ticket').then(m => m.CreateTicket),
      },
      {
    path: 'tickets/:id',
    loadComponent: () =>
      import('./features/tickets/ticket-detail/ticket-detail')
        .then(m => m.TicketDetail),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];