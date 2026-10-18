import { Component, Input, computed, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { httpResource } from '@angular/common/http';
import { TicketListItem, TicketStatus, TicketPriority } from '../../../core/models/ticket.model';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

export interface DashboardTicketFilters {
  status?: TicketStatus;
  priority?: TicketPriority;
  categoryId?: string;
  search?: string;
  assignedToId?: string;
  unassignedOnly?: boolean;
  excludeStatus?: TicketStatus;
  excludeStatuses?: TicketStatus[];
}

export type TicketSortColumn = 'title' | 'status' | 'priority' | 'updated';

@Component({
  selector: 'app-dashboard-ticket-pane',
  standalone: true,
  imports: [CommonModule, RouterLink, EmptyStateComponent],
  templateUrl: './dashboard-ticket-pane.component.html',
  styleUrl: './dashboard-ticket-pane.component.scss',
})
export class DashboardTicketPaneComponent {
  @Input() title: string = 'Tickets';
  @Input() set filters(value: DashboardTicketFilters) {
    // Parents pass a fresh object literal on every change detection cycle, so
    // only react when the filter content actually changes.
    const key = JSON.stringify(value ?? {});
    if (key === this.filtersKey) return;
    this.filtersKey = key;
    this.filtersSignal.set(value ?? {});
  }
  @Input() emptyMessage: string = 'No tickets to display.';
  @Input() emptyTitle: string = 'No tickets found';
  @Input() emptyIcon: string = '📋';
  @Input() pageSize: number = 9;
  // Hosts that already fix the status (e.g. a single-status pane) hide the
  // redundant status dropdown.
  @Input() showStatusFilter: boolean = true;
  // Hosts that can resolve owners (admin) enable the assignee refinement.
  @Input() showAssigneeFilter: boolean = false;
  @Input() assignees: ReadonlyArray<{ id: string; fullName: string }> = [];

  protected readonly TicketStatus = TicketStatus;
  protected readonly TicketPriority = TicketPriority;

  protected readonly filtersSignal = signal<DashboardTicketFilters>({});
  protected readonly sortBy = signal<TicketSortColumn>('updated');
  protected readonly sortDirection = signal<'asc' | 'desc'>('desc');
  protected readonly page = signal(1);

  // User-driven refinements shown in the pane toolbar, layered on top of the
  // filters the host dashboard passes in.
  protected readonly searchTerm = signal('');
  protected readonly statusSelection = signal<TicketStatus | null>(null);
  protected readonly prioritySelection = signal<TicketPriority | null>(null);
  protected readonly assigneeSelection = signal('');

  protected readonly statusOptions = [
    TicketStatus.New,
    TicketStatus.Assigned,
    TicketStatus.InProgress,
    TicketStatus.Resolved,
    TicketStatus.Closed,
    TicketStatus.Cancelled,
  ];
  protected readonly priorityOptions = [
    TicketPriority.Low,
    TicketPriority.Medium,
    TicketPriority.High,
    TicketPriority.Urgent,
  ];

  protected readonly skeletonRows = [1, 2, 3, 4, 5];

  private filtersKey = '';

  // Base filters combined with whatever the user picked in the toolbar.
  private readonly effectiveFilters = computed<DashboardTicketFilters>(() => {
    const filters: DashboardTicketFilters = { ...this.filtersSignal() };

    const term = this.searchTerm().trim();
    if (term) filters.search = term;

    const status = this.statusSelection();
    if (status !== null) {
      filters.status = status;
      filters.excludeStatus = undefined;
      filters.excludeStatuses = undefined;
    }

    const priority = this.prioritySelection();
    if (priority !== null) filters.priority = priority;

    const assignee = this.assigneeSelection();
    if (assignee === 'unassigned') {
      filters.unassignedOnly = true;
      filters.assignedToId = undefined;
    } else if (assignee) {
      filters.assignedToId = assignee;
      filters.unassignedOnly = false;
    }

    return filters;
  });

  protected readonly hasActiveFilters = computed(() =>
    this.searchTerm().trim() !== '' ||
    this.statusSelection() !== null ||
    this.prioritySelection() !== null ||
    this.assigneeSelection() !== '');

  protected readonly tickets = httpResource<TicketListItem[]>(
    () => this.buildTicketUrl(),
    { defaultValue: [] },
  );

  private readonly queryKey = computed(() =>
    JSON.stringify([this.effectiveFilters(), this.sortBy(), this.sortDirection()]));

  protected readonly totalCount = computed(() => this.tickets.value().length);

  protected readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  protected readonly pagedTickets = computed(() => {
    const start = (this.page() - 1) * this.pageSize;
    return this.tickets.value().slice(start, start + this.pageSize);
  });

  protected readonly rangeStart = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize + 1);

  protected readonly rangeEnd = computed(() =>
    Math.min(this.page() * this.pageSize, this.totalCount()));

  constructor() {
    // Any change to the query (filters or sorting) restarts the list at page 1.
    effect(() => {
      this.queryKey();
      this.page.set(1);
    });
  }

  private buildTicketUrl(): string {
    const filters = this.effectiveFilters();
    const params = new URLSearchParams();

    if (filters.status !== undefined) params.append('status', filters.status.toString());
    if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
    if (filters.categoryId) params.append('categoryId', filters.categoryId);
    if (filters.search) params.append('search', filters.search);
    if (filters.assignedToId) params.append('assignedToId', filters.assignedToId);
    if (filters.unassignedOnly) params.append('unassignedOnly', 'true');
    if (filters.excludeStatus !== undefined) params.append('excludeStatus', filters.excludeStatus.toString());
    if (filters.excludeStatuses && filters.excludeStatuses.length > 0) {
      params.append('excludeStatuses', filters.excludeStatuses.join(','));
    }
    params.append('sortBy', this.sortBy());
    params.append('sortDirection', this.sortDirection());

    return `/api/tickets?${params.toString()}`;
  }

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: TicketPriority): string {
    return TicketPriority[priority];
  }

  protected statusClass(status: TicketStatus): string {
    return `status--${TicketStatus[status].toLowerCase()}`;
  }

  protected priorityClass(priority: TicketPriority): string {
    return `priority--${TicketPriority[priority].toLowerCase()}`;
  }

  protected sort(column: TicketSortColumn): void {
    if (this.sortBy() === column) {
      this.sortDirection.update(direction => (direction === 'asc' ? 'desc' : 'asc'));
      return;
    }
    this.sortBy.set(column);
    this.sortDirection.set(column === 'title' ? 'asc' : 'desc');
  }

  protected sortIndicator(column: TicketSortColumn): string {
    if (this.sortBy() !== column) return '';
    return this.sortDirection() === 'asc' ? '↑' : '↓';
  }

  protected setPage(next: number): void {
    this.page.set(Math.min(Math.max(1, next), this.totalPages()));
  }

  protected onSearchInput(event: Event): void {
    this.searchTerm.set((event.target as HTMLInputElement).value);
  }

  protected onStatusChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.statusSelection.set(value === '' ? null : (Number(value) as TicketStatus));
  }

  protected onPriorityChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.prioritySelection.set(value === '' ? null : (Number(value) as TicketPriority));
  }

  protected onAssigneeChange(event: Event): void {
    this.assigneeSelection.set((event.target as HTMLSelectElement).value);
  }

  protected clearFilters(): void {
    this.searchTerm.set('');
    this.statusSelection.set(null);
    this.prioritySelection.set(null);
    this.assigneeSelection.set('');
  }
}
