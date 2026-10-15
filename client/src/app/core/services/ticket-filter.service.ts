import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { TicketStatus, TicketPriority } from '../models/ticket.model';

export interface TicketFilters {
  status?: TicketStatus;
  priority?: TicketPriority;
  categoryId?: string;
  search?: string;
}

@Injectable({ providedIn: 'root' })
export class TicketFilterService {
  private readonly statusFilter = signal<TicketStatus | null>(null);
  private readonly priorityFilter = signal<TicketPriority | null>(null);
  private readonly categoryFilter = signal<string | null>(null);
  private readonly searchFilter = signal<string>('');

  readonly status$ = this.statusFilter.asReadonly();
  readonly priority$ = this.priorityFilter.asReadonly();
  readonly category$ = this.categoryFilter.asReadonly();
  readonly search$ = this.searchFilter.asReadonly();

  readonly hasActiveFilters = computed(() => {
    return this.statusFilter() !== null ||
           this.priorityFilter() !== null ||
           this.categoryFilter() !== null ||
           this.searchFilter().trim() !== '';
  });

  constructor(private router: Router) {}

  setStatus(status: TicketStatus | null) {
    this.statusFilter.set(status);
  }

  setPriority(priority: TicketPriority | null) {
    this.priorityFilter.set(priority);
  }

  setCategory(categoryId: string | null) {
    this.categoryFilter.set(categoryId);
  }

  setSearch(query: string) {
    this.searchFilter.set(query);
  }

  reset() {
    this.statusFilter.set(null);
    this.priorityFilter.set(null);
    this.categoryFilter.set(null);
    this.searchFilter.set('');
  }

  getFilters(): TicketFilters {
    const filters: TicketFilters = {};
    if (this.statusFilter() !== null) filters.status = this.statusFilter()!;
    if (this.priorityFilter() !== null) filters.priority = this.priorityFilter()!;
    if (this.categoryFilter()) filters.categoryId = this.categoryFilter()!;
    if (this.searchFilter().trim()) filters.search = this.searchFilter();
    return filters;
  }

  buildQueryString(): string {
    const filters = this.getFilters();
    const params = new URLSearchParams();
    
    if (filters.status !== undefined) params.append('status', filters.status.toString());
    if (filters.priority !== undefined) params.append('priority', filters.priority.toString());
    if (filters.categoryId) params.append('categoryId', filters.categoryId);
    if (filters.search) params.append('search', filters.search);

    const queryString = params.toString();
    return queryString ? `?${queryString}` : '';
  }
}
