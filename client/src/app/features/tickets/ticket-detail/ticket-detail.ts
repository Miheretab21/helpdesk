import { Component, inject, input, signal, computed } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { TicketService } from '../../../core/services/ticket.service';
import { CommentService } from '../../../core/services/comment.service';
import { AuthService } from '../../../core/services/auth.service';
import { TicketDetail as TicketDetailDto, TicketPriority, TicketStatus } from '../../../core/models/ticket.model';
import { UserRole } from '../../../core/models/user.model';
import { Comment } from '../../../core/models/comment.model';

@Component({
  selector: 'app-ticket-detail',
  imports: [DatePipe, FormsModule],
  templateUrl: './ticket-detail.html',
  styleUrl: './ticket-detail.scss',
})
export class TicketDetail {
  private readonly commentService = inject(CommentService);
  private readonly ticketService = inject(TicketService);
  protected readonly auth = inject(AuthService);

  readonly id = input.required<string>();

  protected readonly ticket = httpResource<TicketDetailDto>(
    () => `/api/tickets/${this.id()}`,
  );

  protected readonly comments = httpResource<Comment[]>(
    () => `/api/tickets/${this.id()}/comments`,
    { defaultValue: [] },
  );

  protected readonly newComment = signal('');
  protected readonly submitting = signal(false);
  protected readonly submitError = signal<string | null>(null);
  protected readonly actionError = signal<string | null>(null);
  protected readonly actionBusy = signal(false);

  protected readonly TicketStatus = TicketStatus;
  protected readonly UserRole = UserRole;

  // ---------------------------------------------------------------------
  // Role-based visibility (the UI's half of the story; the API enforces it).
  // ---------------------------------------------------------------------

  protected readonly isAgent = computed(() => this.auth.isInRole(UserRole.Agent));
  protected readonly isAdmin = computed(() => this.auth.isInRole(UserRole.Admin));
  protected readonly isEmployee = computed(() => this.auth.isInRole(UserRole.Employee));

  protected readonly currentUserId = computed(() => this.auth.user()?.id ?? '');

  protected readonly canClaim = computed(() => {
    const t = this.ticket.value();
    return !!t && this.isAgent() && t.assignedToId === null;
  });

  protected readonly isAssignedToMe = computed(() => {
    const t = this.ticket.value();
    return !!t && t.assignedToId === this.currentUserId();
  });

  protected readonly canStartWork = computed(() => {
    const t = this.ticket.value();
    if (!t || t.status !== TicketStatus.Assigned) return false;
    return this.isAssignedToMe() || this.isAdmin();
  });

  protected readonly canResolve = computed(() => {
    const t = this.ticket.value();
      if (!t || t.status !== TicketStatus.InProgress) return false;
      return this.isAssignedToMe() || this.isAdmin();
    });

  protected readonly canClose = computed(() => {
    const t = this.ticket.value();
      if (!t || t.status !== TicketStatus.Resolved) return false;
      return this.isAssignedToMe() || this.isAdmin();
});

  // ---------------------------------------------------------------------
  // Labels
  // ---------------------------------------------------------------------

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: TicketPriority): string {
    return TicketPriority[priority];
  }

  // ---------------------------------------------------------------------
  // Comment submission (existing)
  // ---------------------------------------------------------------------

  protected async submitComment(): Promise<void> {
    const body = this.newComment().trim();
    if (!body) return;

    this.submitting.set(true);
    this.submitError.set(null);

    try {
      await firstValueFrom(this.commentService.add(this.id(), body));
      this.newComment.set('');
      this.comments.reload();
    } catch {
      this.submitError.set('Could not add comment.');
    } finally {
      this.submitting.set(false);
    }
  }

  // ---------------------------------------------------------------------
  // Ticket actions
  // ---------------------------------------------------------------------

  protected async claim(): Promise<void> {
    const userId = this.currentUserId();
    if (!userId) return;
    await this.runAction(() => this.ticketService.assign(this.id(), userId));
  }

  protected async startWork(): Promise<void> {
    await this.runAction(() => this.ticketService.changeStatus(this.id(), TicketStatus.InProgress));
  }

  protected async resolve(): Promise<void> {
    await this.runAction(() => this.ticketService.changeStatus(this.id(), TicketStatus.Resolved));
  }

  protected async close(): Promise<void> {
    await this.runAction(() => this.ticketService.changeStatus(this.id(), TicketStatus.Closed));
  }

  private async runAction(action: () => ReturnType<typeof this.ticketService.assign>): Promise<void> {
    if (this.actionBusy()) return;

    this.actionBusy.set(true);
    this.actionError.set(null);

    try {
      await firstValueFrom(action());
      this.ticket.reload();
    } catch {
      this.actionError.set('Action failed. Please try again.');
    } finally {
      this.actionBusy.set(false);
    }
  }
}