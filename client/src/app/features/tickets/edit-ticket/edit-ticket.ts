import { Component, inject, input, signal, effect } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { httpResource } from '@angular/common/http';
import { TicketService, EditTicketRequest } from '../../../core/services/ticket.service';
import { CategoryService } from '../../../core/services/category.service';
import { TicketDetail } from '../../../core/models/ticket.model';

@Component({
  selector: 'app-edit-ticket',
  imports: [FormsModule, RouterLink],
  templateUrl: './edit-ticket.html',
  styleUrl: './edit-ticket.scss',
})
export class EditTicket {
  private readonly ticketService = inject(TicketService);
  private readonly categoryService = inject(CategoryService);
  private readonly router = inject(Router);

  readonly id = input.required<string>();

  protected title = '';
  protected description = '';
  protected categoryId = '';

  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly categories = this.categoryService.all;

  protected readonly ticket = httpResource<TicketDetail>(
    () => `/api/tickets/${this.id()}`,
  );

  constructor() {
    effect(() => {
      const t = this.ticket.value();
      if (t) {
        this.title = t.title;
        this.description = t.description;
        this.categoryId = t.categoryId;
      }
    });
  }

  protected async submit(): Promise<void> {
    if (this.submitting()) return;

    this.submitting.set(true);
    this.error.set(null);

    const request: EditTicketRequest = {
      title: this.title.trim(),
      description: this.description.trim(),
      categoryId: this.categoryId,
    };

    try {
      await firstValueFrom(this.ticketService.edit(this.id(), request));
      await this.router.navigate(['/tickets', this.id()]);
    } catch {
      this.error.set('Could not save changes. Please try again.');
    } finally {
      this.submitting.set(false);
    }
  }
}