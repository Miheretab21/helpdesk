import { Component, inject, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { TicketService, CreateTicketRequest } from '../../../core/services/ticket.service';
import { CategoryService } from '../../../core/services/category.service';
import { TicketPriority } from '../../../core/models/ticket.model';
import { Category } from '../../../core/models/ticket.model';

@Component({
  selector: 'app-create-ticket',
  imports: [FormsModule, RouterLink],
  templateUrl: './create-ticket.html',
  styleUrl: './create-ticket.scss',
})
export class CreateTicket {
  private readonly ticketService = inject(TicketService);
  private readonly categoryService = inject(CategoryService);
  private readonly router = inject(Router);

  protected readonly title = signal('');
  protected readonly description = signal('');
  protected readonly priority = signal<TicketPriority>(TicketPriority.Medium);
  protected readonly categoryId = signal('');

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly priorities = [
    { value: TicketPriority.Low, label: 'Low' },
    { value: TicketPriority.Medium, label: 'Medium' },
    { value: TicketPriority.High, label: 'High' },
    { value: TicketPriority.Urgent, label: 'Urgent' },
  ];

  protected readonly categories = this.categoryService.all;

  protected readonly canSubmit = computed(() =>
    this.title().trim().length > 0 &&
    this.description().trim().length > 0 &&
    this.categoryId().length > 0 &&
    !this.submitting()
  );

  protected async submit(): Promise<void> {
    if (!this.canSubmit()) return;

    this.submitting.set(true);
    this.error.set(null);

    const request: CreateTicketRequest = {
      title: this.title().trim(),
      description: this.description().trim(),
      priority: this.priority(),
      categoryId: this.categoryId(),
    };

    try {
      const result = await firstValueFrom(this.ticketService.create(request));
      await this.router.navigate(['/tickets', result.id]);
    } catch {
      this.error.set('Could not create ticket. Please try again.');
    } finally {
      this.submitting.set(false);
    }
  }

  protected onPriorityChange(event: Event): void {
    const value = parseInt((event.target as HTMLSelectElement).value, 10) as TicketPriority;
    this.priority.set(value);
  }
}