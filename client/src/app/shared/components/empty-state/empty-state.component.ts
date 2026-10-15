import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  template: `
    <div class="empty-state">
      <div class="empty-state__icon">{{ icon() }}</div>
      <h2 class="empty-state__title">{{ title() }}</h2>
      <p class="empty-state__message">{{ message() }}</p>
      <ng-content></ng-content>
    </div>
  `,
  styles: [`
    .empty-state {
      text-align: center;
      padding: 40px 20px;
      color: #666;
    }

    .empty-state__icon {
      font-size: 48px;
      margin-bottom: 16px;
      line-height: 1;
    }

    .empty-state__title {
      font-size: 18px;
      font-weight: 600;
      color: #333;
      margin: 0 0 8px 0;
    }

    .empty-state__message {
      font-size: 14px;
      color: #666;
      margin: 0 0 24px 0;
      max-width: 400px;
      margin-left: auto;
      margin-right: auto;
    }
  `]
})
export class EmptyStateComponent {
  readonly icon = input<string>('📭');
  readonly title = input<string>('Nothing here');
  readonly message = input<string>('');
}
