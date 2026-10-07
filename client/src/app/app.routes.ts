import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/tickets/ticket-list/ticket-list.component')
        .then(m => m.TicketListComponent),
  },
];