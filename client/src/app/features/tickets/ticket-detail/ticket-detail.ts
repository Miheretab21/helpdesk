import { Component, inject, input, signal, computed, effect, HostListener, OnDestroy } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { TicketService } from '../../../core/services/ticket.service';
import { CommentService } from '../../../core/services/comment.service';
import { UserService } from '../../../core/services/user.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { TicketAttachment, TicketAuditEntry, TicketDetail as TicketDetailDto, TicketPriority, TicketStatus } from '../../../core/models/ticket.model';
import { UserRole, User } from '../../../core/models/user.model';
import { Comment } from '../../../core/models/comment.model';
import { RouterLink} from '@angular/router';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-ticket-detail',
  imports: [DatePipe, DecimalPipe, FormsModule, RouterLink, SpinnerComponent],
  templateUrl: './ticket-detail.html',
  styleUrl: './ticket-detail.scss',
})
export class TicketDetail implements OnDestroy {
  private readonly commentService = inject(CommentService);
  protected readonly ticketService = inject(TicketService);
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
  protected readonly attachments = httpResource<TicketAttachment[]>(() => `/api/tickets/${this.id()}/attachments`, { defaultValue: [] });
  protected readonly history = httpResource<TicketAuditEntry[]>(() => `/api/tickets/${this.id()}/history`, { defaultValue: [] });

  protected readonly newComment = signal('');
  protected readonly submitting = signal(false);
  protected readonly submitError = signal<string | null>(null);
  protected readonly actionBusy = signal(false);
  protected readonly selectedAssigneeId = signal<string | null>(null);

  // Inline previews for image attachments, keyed by attachment id.
  private readonly imageUrls = signal<Record<string, string>>({});
  private readonly requestedImageIds = new Set<string>();

  constructor() {
    // Fetch a preview once per image attachment (the list reloads after upload).
    effect(() => {
      for (const attachment of this.attachments.value()) {
        if (!this.isImage(attachment.contentType)) continue;
        if (this.requestedImageIds.has(attachment.id)) continue;
        this.requestedImageIds.add(attachment.id);
        this.loadImagePreview(attachment.id);
      }
    });
  }

  protected isImage(contentType: string): boolean {
    return contentType?.startsWith('image/') ?? false;
  }

  protected imageUrl(attachmentId: string): string | undefined {
    return this.imageUrls()[attachmentId];
  }

  private loadImagePreview(attachmentId: string): void {
    this.ticketService.downloadAttachment(this.id(), attachmentId).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        this.imageUrls.update((map) => ({ ...map, [attachmentId]: url }));
      },
      error: () => this.requestedImageIds.delete(attachmentId),
    });
  }

  ngOnDestroy(): void {
    Object.values(this.imageUrls()).forEach((url) => URL.revokeObjectURL(url));
  }

  // Full-screen image viewer (opened by tapping/clicking a preview).
  protected readonly lightbox = signal<{ url: string; name: string } | null>(null);

  protected openLightbox(url: string, name: string): void {
    this.lightbox.set({ url, name });
  }

  protected closeLightbox(): void {
    this.lightbox.set(null);
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    this.closeLightbox();
  }

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
  protected readonly canReopen = computed(() => {
    const t = this.ticket.value();
    return !!t && t.status === TicketStatus.Closed && (this.isAdmin() || t.createdById === this.currentUserId());
  });
  protected readonly canEscalate = computed(() => {
    const t = this.ticket.value();
    return !!t && !t.escalatedAt && ![TicketStatus.Closed, TicketStatus.Cancelled].includes(t.status) && (this.isAgent() || this.isAdmin());
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
  protected async reopen(): Promise<void> { await this.runAction(() => this.ticketService.reopen(this.id()), 'Ticket reopened'); }
  protected async escalate(): Promise<void> { await this.runAction(() => this.ticketService.escalate(this.id()), 'Ticket escalated'); }
  protected async uploadAttachment(event: Event): Promise<void> {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;
    try { await firstValueFrom(this.ticketService.uploadAttachment(this.id(), file)); this.attachments.reload(); this.history.reload(); this.toast.success('Attachment added'); }
    catch { this.toast.error('Could not upload the attachment.'); }
    finally { (event.target as HTMLInputElement).value = ''; }
  }

  private async runAction(action: () => ReturnType<typeof this.ticketService.assign>, successMessage: string): Promise<void> {
    if (this.actionBusy()) return;

    this.actionBusy.set(true);

    try {
      await firstValueFrom(action());
      this.ticket.reload();
      this.history.reload();
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
