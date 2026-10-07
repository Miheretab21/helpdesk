import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { TicketDetail, TicketListItem } from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly api = inject(ApiService);

  list() {
    return this.api.get<TicketListItem[]>('/api/tickets');
  }
  // getById(id: string) {
  // return this.api.get<TicketListItem>(`/api/tickets/${id}`);
  // }
  getById(id: string) {
  return this.api.get<TicketDetail>(`/api/tickets/${id}`);
  }
}