import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { TicketStatus, TicketPriority } from '../../../core/models/ticket.model';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { DashboardTicketPaneComponent } from '../dashboard-ticket-pane/dashboard-ticket-pane.component';

export interface AgentDashboardData {
  myTicketsByStatus: Record<number, number>;
  unassignedQueueCount: number;
  myRecentActivity: Array<{
    id: string;
    title: string;
    status: number;
    priority: number;
    updatedAt: string;
  }>;
}

export enum AgentDashboardPane {
  MyTickets = 'my-tickets',
  UnassignedQueue = 'unassigned',
  RecentActivity = 'recent',
}

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule, SpinnerComponent, DashboardTicketPaneComponent],
  templateUrl: './agent-dashboard.component.html',
  styleUrl: './agent-dashboard.component.scss',
})
export class AgentDashboardComponent {
  protected readonly TicketStatus = TicketStatus;
  protected readonly TicketPriority = TicketPriority;
  protected readonly AgentDashboardPane = AgentDashboardPane;

  // Track which pane is active (for split-pane display)
  protected readonly activePane = signal<AgentDashboardPane>(AgentDashboardPane.MyTickets);

  // Fetch agent dashboard data
  protected readonly agentData = httpResource<AgentDashboardData>(
    () => '/api/dashboard/agent',
  );

  protected getStatusCount(statusCode: number): number {
    const data = this.agentData.value();
    if (!data || !data.myTicketsByStatus) return 0;
    return data.myTicketsByStatus[statusCode] || 0;
  }

  protected getTotalCount(): number {
    const data = this.agentData.value();
    if (!data || !data.myTicketsByStatus) return 0;
    return Object.values(data.myTicketsByStatus).reduce((sum: any, count: any) => sum + (count || 0), 0);
  }

  protected statusLabel(status: number): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: number): string {
    return TicketPriority[priority];
  }

  protected setActivePane(pane: AgentDashboardPane): void {
    this.activePane.set(pane);
  }
}

