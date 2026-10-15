import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { httpResource } from '@angular/common/http';
import { DashboardService, DashboardMetrics } from '../../core/services/dashboard.service';
import { TicketStatus, TicketPriority } from '../../core/models/ticket.model';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../core/models/user.model';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SpinnerComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  private readonly dashboardService = inject(DashboardService);
  protected readonly auth = inject(AuthService);

  protected readonly metrics = httpResource<DashboardMetrics>(
    () => '/api/dashboard/metrics',
  );

  protected readonly TicketStatus = TicketStatus;
  protected readonly TicketPriority = TicketPriority;
  protected readonly UserRole = UserRole;

  protected readonly isAgent = computed(() => this.auth.isInRole(UserRole.Agent));
  protected readonly isAdmin = computed(() => this.auth.isInRole(UserRole.Admin));

  protected readonly statusCounts = computed(() => {
    const m = this.metrics.value();
    if (!m) return [];
    return [
      { status: TicketStatus.New, label: 'New', count: m.ticketCountByStatus[TicketStatus.New] || 0 },
      { status: TicketStatus.Assigned, label: 'Assigned', count: m.ticketCountByStatus[TicketStatus.Assigned] || 0 },
      { status: TicketStatus.InProgress, label: 'In Progress', count: m.ticketCountByStatus[TicketStatus.InProgress] || 0 },
      { status: TicketStatus.Resolved, label: 'Resolved', count: m.ticketCountByStatus[TicketStatus.Resolved] || 0 },
      { status: TicketStatus.Closed, label: 'Closed', count: m.ticketCountByStatus[TicketStatus.Closed] || 0 },
      { status: TicketStatus.Cancelled, label: 'Cancelled', count: m.ticketCountByStatus[TicketStatus.Cancelled] || 0 },
    ];
  });

  protected readonly priorityCounts = computed(() => {
    const m = this.metrics.value();
    if (!m) return [];
    return [
      { priority: TicketPriority.Low, label: 'Low', count: m.ticketCountByPriority[TicketPriority.Low] || 0 },
      { priority: TicketPriority.Medium, label: 'Medium', count: m.ticketCountByPriority[TicketPriority.Medium] || 0 },
      { priority: TicketPriority.High, label: 'High', count: m.ticketCountByPriority[TicketPriority.High] || 0 },
      { priority: TicketPriority.Urgent, label: 'Urgent', count: m.ticketCountByPriority[TicketPriority.Urgent] || 0 },
    ];
  });

  protected readonly totalTickets = computed(() => {
    const counts = this.statusCounts();
    return counts.reduce((sum, item) => sum + item.count, 0);
  });

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: TicketPriority): string {
    return TicketPriority[priority];
  }
}
