import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Comment } from '../models/comment.model';

@Injectable({ providedIn: 'root' })
export class CommentService {
  private readonly api = inject(ApiService);

  listForTicket(ticketId: string) {
    return this.api.get<Comment[]>(`/api/tickets/${ticketId}/comments`);
  }

  add(ticketId: string, body: string) {
    return this.api.post<{ id: string }>(`/api/tickets/${ticketId}/comments`, { body });
  }
}