import { Component, inject, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { TicketStatus, TicketPriority, Category } from '../../../core/models/ticket.model';
import { User, UserRole } from '../../../core/models/user.model';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { DashboardTicketPaneComponent, DashboardTicketFilters } from '../dashboard-ticket-pane/dashboard-ticket-pane.component';
import { UserManagementComponent } from '../../admin/user-management/user-management.component';

export interface AdminDashboardData {
  allTicketsByStatus: Record<number, number>;
  ticketsByCategory: Record<number, number>;
  workloadByAgent: Record<string, number>;
  unassignedBacklog: number;
  averageTimeToClose: number;
  recentActivitySystem: Array<{
    id: string;
    title: string;
    status: number;
    priority: number;
    updatedAt: string;
  }>;
  topAgentsThisWeek: Array<{ name: string; resolvedCount: number }>;
}

export enum AdminDashboardPane {
  Assigned = 'assigned',
  All = 'all',
  Unassigned = 'unassigned',
  ByStatus = 'by-status',
  RecentActivity = 'recent',
  UserManagement = 'users',
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, SpinnerComponent, DashboardTicketPaneComponent, UserManagementComponent],
  templateUrl: './admin-dashboard.component.html',
  styleUrl: './admin-dashboard.component.scss',
})
export class AdminDashboardComponent {
  protected readonly TicketStatus = TicketStatus;
  protected readonly TicketPriority = TicketPriority;
  protected readonly AdminDashboardPane = AdminDashboardPane;

  // Track which pane is active (for split-pane display)
  protected readonly activePane = signal<AdminDashboardPane>(AdminDashboardPane.Assigned);

  // Track selected status filter for ByStatus pane
  protected readonly selectedStatus = signal<TicketStatus | null>(null);

  // Fetch admin dashboard data
  protected readonly adminData = httpResource<AdminDashboardData>(
    () => '/api/dashboard/admin',
  );

  // Fetch categories for mapping IDs to names
  protected readonly categories = httpResource<Category[]>(
    () => '/api/categories',
    { defaultValue: [] },
  );

  // Agents and admins that can own a ticket (admin-only endpoint).
  protected readonly users = httpResource<User[]>(
    () => '/api/users',
    { defaultValue: [] },
  );

  protected readonly assignees = computed(() =>
    this.users.value().filter(user => user.role !== UserRole.Employee),
  );

  protected readonly statusCounts = computed(() => {
    const data = this.adminData.value();
    if (!data) return [];
    return [
      { status: TicketStatus.New, label: 'New', count: data.allTicketsByStatus[TicketStatus.New] || 0 },
      { status: TicketStatus.Assigned, label: 'Assigned', count: data.allTicketsByStatus[TicketStatus.Assigned] || 0 },
      { status: TicketStatus.InProgress, label: 'In Progress', count: data.allTicketsByStatus[TicketStatus.InProgress] || 0 },
      { status: TicketStatus.Resolved, label: 'Resolved', count: data.allTicketsByStatus[TicketStatus.Resolved] || 0 },
      { status: TicketStatus.Closed, label: 'Closed', count: data.allTicketsByStatus[TicketStatus.Closed] || 0 },
      { status: TicketStatus.Cancelled, label: 'Cancelled', count: data.allTicketsByStatus[TicketStatus.Cancelled] || 0 },
    ];
  });

  // Status list for the sidebar, minus Assigned (promoted to a top-level queue).
  protected readonly statusFilterItems = computed(() =>
    this.statusCounts().filter(item => item.status !== TicketStatus.Assigned),
  );

  protected getCategoryBreakdownList(): Array<{ catId: number; label: string; count: number }> {
    const data = this.adminData.value();
    const cats = this.categories.value();
    if (!data || !data.ticketsByCategory) return [];
    
    // Create a list with indices matching the backend order
    const categoryList = cats;
    
    return Object.entries(data.ticketsByCategory)
      .map(([indexStr, count]) => {
        const index = parseInt(indexStr);
        const categoryName = categoryList[index]?.name || `Category ${index}`;
        return {
          catId: index,
          label: categoryName,
          count: count as number,
        };
      })
      .sort((a, b) => b.count - a.count);
  }

  protected getWorkloadList(): Array<{ agent: string; count: number }> {
    const data = this.adminData.value();
    if (!data || !data.workloadByAgent) return [];
    return Object.entries(data.workloadByAgent).map(([agent, count]) => ({
      agent,
      count: count as number,
    }));
  }

  protected readonly maxWorkload = computed(() => {
    const workload = this.getWorkloadList();
    return workload.length > 0 ? Math.max(...workload.map(w => w.count)) : 1;
  });

  // Filters implied by the active pane, before the toolbar refinements.
  private readonly baseFilters = computed<DashboardTicketFilters>(() => {
    switch (this.activePane()) {
      case AdminDashboardPane.Assigned:
        return { status: TicketStatus.Assigned };
      case AdminDashboardPane.Unassigned:
        return { status: TicketStatus.New, unassignedOnly: true };
      case AdminDashboardPane.ByStatus:
        return this.selectedStatus() !== null ? { status: this.selectedStatus()! } : {};
      default:
        return {};
    }
  });

  // Filters implied by the active pane. The pane's own toolbar layers the
  // search / status / priority / assignee refinements on top.
  protected readonly ticketFilters = computed<DashboardTicketFilters>(() => ({
    ...this.baseFilters(),
  }));

  protected statusLabel(status: number): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: number): string {
    return TicketPriority[priority];
  }

  protected setActivePane(pane: AdminDashboardPane): void {
    this.activePane.set(pane);
  }

  protected selectStatus(status: TicketStatus): void {
    this.activePane.set(AdminDashboardPane.ByStatus);
    this.selectedStatus.set(status);
  }
}