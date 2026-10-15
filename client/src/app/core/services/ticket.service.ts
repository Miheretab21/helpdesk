import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { TicketDetail, TicketListItem, TicketPriority, TicketStatus } from '../models/ticket.model';

export interface CreateTicketRequest {
  title: string;
  description: string;
  priority: TicketPriority;
  categoryId: string;
}
export interface EditTicketRequest {
  title: string;
  description: string;
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
  cancel(ticketId: string) {
    return this.api.post<void>(`/api/tickets/${ticketId}/cancel`, {});
  }
  edit(ticketId: string, request: EditTicketRequest) {
    return this.api.put<void>(`/api/tickets/${ticketId}`, request);
  }
  changeStatus(ticketId: string, targetStatus: TicketStatus) {
    return this.api.post<void>(`/api/tickets/${ticketId}/status`, { targetStatus });
  }
}