import { Component, computed, inject, effect } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { TicketService, TicketFilters } from '../../../core/services/ticket.service';
import { TicketListItem, TicketPriority, TicketStatus } from '../../../core/models/ticket.model';
import { CategoryService } from '../../../core/services/category.service';
import { Category } from '../../../core/models/ticket.model';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { signal } from '@angular/core';
import { debounceTime, Subject } from 'rxjs';

@Component({
  selector: 'app-ticket-list',
  imports: [DatePipe, RouterLink, SpinnerComponent, EmptyStateComponent, FormsModule],
  templateUrl: './ticket-list.component.html',
  styleUrl: './ticket-list.component.scss',
})
export class TicketListComponent {
  private readonly ticketService = inject(TicketService);
  private readonly categoryService = inject(CategoryService);
  private readonly route = inject(ActivatedRoute);

  protected readonly TicketStatus = TicketStatus;
  protected readonly TicketPriority = TicketPriority;

  // Filters
  protected readonly statusFilter = signal<TicketStatus | null>(null);
  protected readonly priorityFilter = signal<TicketPriority | null>(null);
  protected readonly categoryFilter = signal<string | null>(null);
  protected readonly searchFilter = signal<string>('');
  protected readonly unassignedOnlyFilter = signal<boolean>(false);

  private readonly searchSubject = new Subject<string>();

  protected readonly categories = httpResource<Category[]>(
    () => '/api/categories',
    { defaultValue: [] },
  );

  protected readonly hasActiveFilters = computed(() => {
    return this.statusFilter() !== null ||
           this.priorityFilter() !== null ||
           this.categoryFilter() !== null ||
           this.searchFilter().trim() !== '' ||
           this.unassignedOnlyFilter();
  });

  protected readonly currentFilters = computed<TicketFilters>(() => {
    const filters: TicketFilters = {};
    if (this.statusFilter() !== null) filters.status = this.statusFilter()!;
    if (this.priorityFilter() !== null) filters.priority = this.priorityFilter()!;
    if (this.categoryFilter()) filters.categoryId = this.categoryFilter()!;
    if (this.searchFilter().trim()) filters.search = this.searchFilter();
    return filters;
  });

  protected readonly tickets = httpResource<TicketListItem[]>(
    () => {
      const filters = this.currentFilters();
      // Build URL with query params
      let url = '/api/tickets';
      const params = new URLSearchParams();
      if (filters.status !== undefined) params.append('status', filters.status.toString());
      if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
      if (filters.categoryId) params.append('categoryId', filters.categoryId);
      if (filters.search) params.append('search', filters.search);
      // Add unassigned filter if needed
      if (this.unassignedOnlyFilter()) params.append('unassignedOnly', 'true');
      const queryString = params.toString();
      if (queryString) url += `?${queryString}`;
      return url;
    },
    { defaultValue: [] },
  );

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: TicketPriority): string {
    return TicketPriority[priority];
  }

  protected resetFilters() {
    this.statusFilter.set(null);
    this.priorityFilter.set(null);
    this.categoryFilter.set(null);
    this.searchFilter.set('');
    this.unassignedOnlyFilter.set(false);
  }

  protected auth = inject(AuthService);
  protected readonly UserRole = UserRole;
  protected readonly canCreateTicket = computed(() => this.auth.isInRole(UserRole.Employee));
  protected readonly isEmployee = computed(() => this.auth.isInRole(UserRole.Employee));
  protected readonly isAgent = computed(() => this.auth.isInRole(UserRole.Agent));
  protected readonly isAdmin = computed(() => this.auth.isInRole(UserRole.Admin));
  protected readonly shouldShowPriorityFilter = computed(() => !this.isEmployee());

  constructor() {
    // Set up debounced search
    this.searchSubject.pipe(debounceTime(500)).subscribe(query => {
      this.searchFilter.set(query);
    });

    // Set up search input listener
    effect(() => {
      // This effect runs whenever currentFilters changes, causing tickets to reload
      this.currentFilters();
    });

    // Read query params from URL
    effect(() => {
      this.route.queryParams.subscribe(params => {
        // Check for unassigned filter (when assignedToId=null is passed)
        if (params['assignedToId'] === 'null') {
          this.unassignedOnlyFilter.set(true);
        } else {
          this.unassignedOnlyFilter.set(false);
        }
        
        // Check for status filter in query params
        if (params['status']) {
          this.statusFilter.set(parseInt(params['status']));
        }
      });
    });
  }

  protected onSearchInput(event: Event) {
    const target = event.target as HTMLInputElement;
    this.searchSubject.next(target.value);
  }
}