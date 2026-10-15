import { Component, computed, inject } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { TicketService } from '../../../core/services/ticket.service';
import { TicketListItem, TicketPriority, TicketStatus } from '../../../core/models/ticket.model';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';

@Component({
  selector: 'app-ticket-list',
  imports: [DatePipe, RouterLink],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.scss',
})
export class TicketListComponent {
  private readonly ticketService = inject(TicketService);

  protected readonly tickets = httpResource<TicketListItem[]>(
    () => '/api/tickets',
    { defaultValue: [] },
  );

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: TicketPriority): string {
    return TicketPriority[priority];
  }
protected readonly auth = inject(AuthService);
protected readonly UserRole = UserRole;
protected readonly canCreateTicket = computed(() => this.auth.isInRole(UserRole.Employee));
}