import { Component, inject, input, signal } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { CommentService } from '../../../core/services/comment.service';
import { TicketDetail as TicketDetailDto, TicketPriority, TicketStatus } from '../../../core/models/ticket.model';
import { Comment } from '../../../core/models/comment.model';

@Component({
  selector: 'app-ticket-detail',
  imports: [DatePipe, FormsModule],
  templateUrl: './ticket-detail.html',
  styleUrl: './ticket-detail.scss',
})
export class TicketDetail {
  private readonly commentService = inject(CommentService);

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

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }

  protected priorityLabel(priority: TicketPriority): string {
    return TicketPriority[priority];
  }

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
}