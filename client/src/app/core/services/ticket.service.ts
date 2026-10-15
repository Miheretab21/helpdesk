import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { TicketDetail, TicketListItem, TicketPriority, TicketStatus } from '../models/ticket.model';

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: TicketPriority;
  categoryId: string;
}

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly api = inject(ApiService);

  list() {
    return this.api.get<TicketListItem[]>('/api/tickets');
  }

  getById(id: string) {
    return this.api.get<TicketDetail>(`/api/tickets/${id}`);
  }

  create(request: CreateTicketRequest) {
    return this.api.post<{ id: string }>('/api/tickets', request);
  }
  assign(ticketId: string, assignToUserId: string) {
    return this.api.post<void>(`/api/tickets/${ticketId}/assign`, { assignToUserId });
  }
  changeStatus(ticketId: string, targetStatus: TicketStatus) {
    return this.api.post<void>(`/api/tickets/${ticketId}/status`, { targetStatus });
  }
}