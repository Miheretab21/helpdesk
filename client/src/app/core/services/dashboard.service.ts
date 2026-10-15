import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { TicketListItem } from '../models/ticket.model';

export interface DashboardMetrics {
  ticketCountByStatus: Record<number, number>;
  ticketCountByPriority: Record<number, number>;
  recentActivity: TicketListItem[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly api = inject(ApiService);

  getMetrics() {
    return this.api.get<DashboardMetrics>('/api/dashboard/metrics');
  }
}
