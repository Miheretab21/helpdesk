import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { TicketListItem } from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly api = inject(ApiService);

  list() {
    return this.api.get<TicketListItem[]>('/api/tickets');
  }
}