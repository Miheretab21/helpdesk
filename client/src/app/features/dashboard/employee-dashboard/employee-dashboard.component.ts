import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { httpResource } from '@angular/common/http';
import { TicketStatus, TicketPriority } from '../../../core/models/ticket.model';
import { AuthService } from '../../../core/services/auth.service';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { DashboardTicketPaneComponent } from '../dashboard-ticket-pane/dashboard-ticket-pane.component';

export interface EmployeeDashboardData {
  myTicketsByStatus: Record<number, number>;
  myRecentActivity: Array<{
    id: string;
    title: string;
    status: number;
    priority: number;
    updatedAt: string;
  }>;
}

export enum EmployeeDashboardPane {
  MyTickets = 'my-tickets',
  RecentActivity = 'recent',
}

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [CommonModule, SpinnerComponent, DashboardTicketPaneComponent],
  templateUrl: './employee-dashboard.component.html',
  styleUrl: './employee-dashboard.component.scss',
})
export class EmployeeDashboardComponent {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly TicketStatus = TicketStatus;
  protected readonly TicketPriority = TicketPriority;
  protected readonly EmployeeDashboardPane = EmployeeDashboardPane;

  // Track which pane is active (for split-pane display)
  protected readonly activePane = signal<EmployeeDashboardPane>(EmployeeDashboardPane.MyTickets);

  // Fetch employee's tickets
  protected readonly myTickets = httpResource<EmployeeDashboardData>(
    () => '/api/dashboard/employee',
  );

  protected getStatusCount(statusCode: number): number {
    const data = this.myTickets.value();
    if (!data || !data.myTicketsByStatus) return 0;
    return data.myTicketsByStatus[statusCode] || 0;
  }

  protected getTotalCount(): number {
    const data = this.myTickets.value();
    if (!data || !data.myTicketsByStatus) return 0;
    return Object.values(data.myTicketsByStatus).reduce((sum, count) => sum + (count || 0), 0);
  }

  protected statusLabel(status: number): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: number): string {
    return TicketPriority[priority];
  }

  protected createTicket(): void {
    this.router.navigate(['/tickets/new']);
  }

  protected setActivePane(pane: EmployeeDashboardPane): void {
    this.activePane.set(pane);
  }
}

