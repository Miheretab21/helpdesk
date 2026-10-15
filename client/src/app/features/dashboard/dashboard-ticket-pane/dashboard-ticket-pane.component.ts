import { Component, Input, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { httpResource } from '@angular/common/http';
import { TicketListItem, TicketStatus, TicketPriority } from '../../../core/models/ticket.model';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

export interface DashboardTicketFilters {
  status?: TicketStatus;
  priority?: TicketPriority;
  categoryId?: string;
  search?: string;
  assignedToMe?: boolean;
  unassignedOnly?: boolean;
  excludeStatus?: TicketStatus;
  excludeStatuses?: TicketStatus[];
}

@Component({
  selector: 'app-dashboard-ticket-pane',
  standalone: true,
  imports: [CommonModule, RouterLink, SpinnerComponent],
  templateUrl: './dashboard-ticket-pane.component.html',
  styleUrl: './dashboard-ticket-pane.component.scss',
})
export class DashboardTicketPaneComponent {
  @Input() title: string = 'Tickets';
  @Input() set filters(value: DashboardTicketFilters) {
    this.filtersSignal.set(value);
  }
  @Input() emptyMessage: string = 'No tickets to display.';

  protected readonly TicketStatus = TicketStatus;
  protected readonly TicketPriority = TicketPriority;

  protected readonly filtersSignal = signal<DashboardTicketFilters>({});

  protected readonly tickets = httpResource<TicketListItem[]>(
    () => this.buildTicketUrl(),
    { defaultValue: [] },
  );

  private buildTicketUrl(): string {
    const filters = this.filtersSignal();
    let url = '/api/tickets';
    const params = new URLSearchParams();

    if (filters.status !== undefined) {
      params.append('status', filters.status.toString());
    }
    if (filters.priority !== undefined) {
      params.append('priority', filters.priority.toString());
    }
    if (filters.categoryId) {
      params.append('categoryId', filters.categoryId);
    }
    if (filters.search) {
      params.append('search', filters.search);
    }
    if (filters.unassignedOnly) {
      params.append('unassignedOnly', 'true');
    }
    if (filters.excludeStatus !== undefined) {
      params.append('excludeStatus', filters.excludeStatus.toString());
    }
    if (filters.excludeStatuses && filters.excludeStatuses.length > 0) {
      params.append('excludeStatuses', filters.excludeStatuses.join(','));
    }

    const queryString = params.toString();
    return queryString ? `${url}?${queryString}` : url;
  }

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: TicketPriority): string {
    return TicketPriority[priority];
  }

  protected getPriorityClass(priority: TicketPriority): string {
    switch (priority) {
      case TicketPriority.High:
        return 'priority--high';
      case TicketPriority.Medium:
        return 'priority--medium';
      case TicketPriority.Low:
        return 'priority--low';
      default:
        return 'priority--default';
    }
  }
}
