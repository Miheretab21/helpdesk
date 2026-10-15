import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, EmptyStateComponent],
  template: `
    <div class="not-found">
      <app-empty-state
        icon="🔍"
        title="Page Not Found"
        message="The page you're looking for doesn't exist. It might have been moved or deleted.">
        <a routerLink="/" class="btn btn--primary">Back to Home</a>
      </app-empty-state>
    </div>
  `,
  styles: [`
    .not-found {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 60vh;
    }
  `]
})
export class NotFoundComponent {}
