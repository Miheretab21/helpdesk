import { Component, inject, input, signal, computed } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { TicketService } from '../../../core/services/ticket.service';
import { CommentService } from '../../../core/services/comment.service';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { TicketDetail as TicketDetailDto, TicketPriority, TicketStatus } from '../../../core/models/ticket.model';
import { UserRole, User } from '../../../core/models/user.model';
import { Comment } from '../../../core/models/comment.model';
import { RouterLink} from '@angular/router';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-ticket-detail',
  imports: [DatePipe, FormsModule, RouterLink, SpinnerComponent],
  templateUrl: './ticket-detail.html',
  styleUrl: './ticket-detail.scss',
})
export class TicketDetail {
  private readonly commentService = inject(CommentService);
  private readonly ticketService = inject(TicketService);
  private readonly userService = inject(UserService);
  protected readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly id = input.required<string>();

  protected readonly ticket = httpResource<TicketDetailDto>(
    () => `/api/tickets/${this.id()}`,
  );

  protected readonly comments = httpResource<Comment[]>(
    () => `/api/tickets/${this.id()}/comments`,
    { defaultValue: [] },
  );

  protected readonly users = httpResource<User[]>(
    () => '/api/users',
    { defaultValue: [] },
  );

  protected readonly newComment = signal('');
  protected readonly submitting = signal(false);
  protected readonly submitError = signal<string | null>(null);
  protected readonly actionBusy = signal(false);
  protected readonly selectedAssigneeId = signal<string | null>(null);

  protected readonly TicketStatus = TicketStatus;
  protected readonly UserRole = UserRole;

  // ---------------------------------------------------------------------
  // Role-based visibility (the UI's half of the story; the API enforces it).
  // ---------------------------------------------------------------------

  protected readonly isAgent = computed(() => this.auth.isInRole(UserRole.Agent));
  protected readonly isAdmin = computed(() => this.auth.isInRole(UserRole.Admin));
  protected readonly isEmployee = computed(() => this.auth.isInRole(UserRole.Employee));

  protected readonly currentUserId = computed(() => this.auth.user()?.id ?? '');

  protected readonly availableAgents = computed(() => {
    return this.users.value().filter(u => u.role === UserRole.Agent);
  });

  protected readonly canClaim = computed(() => {
  const t = this.ticket.value();
  if (!t) return false;
  if (!this.isAgent()) return false;
  if (t.assignedToId !== null) return false;
  if (t.status !== TicketStatus.New) return false;
  return true;
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
  protected readonly canCancel = computed(() => {
  const t = this.ticket.value();
  if (!t) return false;
  return this.isEmployee() && t.createdById === this.currentUserId() && t.status === TicketStatus.New;
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
  // Admin assign functionality
  // ---------------------------------------------------------------------

  protected async assignToUser(): Promise<void> {
    const userId = this.selectedAssigneeId();
    if (!userId) return;

    await this.runAction(() => this.ticketService.assign(this.id(), userId), `Assigned to agent`);
    this.selectedAssigneeId.set(null);
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
      this.toast.success('Comment posted');
    } catch {
      this.toast.error('Could not add comment.');
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
    await this.runAction(() => this.ticketService.assign(this.id(), userId), 'Ticket claimed');
  }

  protected async startWork(): Promise<void> {
    await this.runAction(() => this.ticketService.changeStatus(this.id(), TicketStatus.InProgress), 'Work started');
  }

  protected async resolve(): Promise<void> {
    await this.runAction(() => this.ticketService.changeStatus(this.id(), TicketStatus.Resolved), 'Ticket resolved');
  }

  protected async close(): Promise<void> {
    await this.runAction(() => this.ticketService.changeStatus(this.id(), TicketStatus.Closed), 'Ticket closed');
  }

  private async runAction(action: () => ReturnType<typeof this.ticketService.assign>, successMessage: string): Promise<void> {
    if (this.actionBusy()) return;

    this.actionBusy.set(true);

    try {
      await firstValueFrom(action());
      this.ticket.reload();
      this.toast.success(successMessage);
    } catch {
      this.toast.error('Action failed. Please try again.');
    } finally {
      this.actionBusy.set(false);
    }
  }
  protected async cancel(): Promise<void> {
  if (!confirm('Cancel this ticket? This cannot be undone.')) return;
  await this.runAction(() => this.ticketService.cancel(this.id()), 'Ticket cancelled');
}
}