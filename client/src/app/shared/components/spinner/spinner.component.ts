import { Component, input } from '@angular/core';

type SpinnerSize = 'small' | 'medium' | 'large';

@Component({
  selector: 'app-spinner',
  standalone: true,
  template: `
    <div [class]="'spinner spinner--' + size()" [attr.aria-label]="label()">
      <div class="spinner__ring"></div>
      <div class="spinner__ring"></div>
      <div class="spinner__ring"></div>
    </div>
  `,
  styles: [`
    .spinner {
      display: inline-block;
    }

    .spinner--small {
      width: 24px;
      height: 24px;
    }

    .spinner--medium {
      width: 40px;
      height: 40px;
    }

    .spinner--large {
      width: 56px;
      height: 56px;
    }

    .spinner__ring {
      display: inline-block;
      position: absolute;
      left: 8px;
      animation: spinner-ring 1.2s cubic-bezier(0.5, 0, 0.5, 1) infinite;
      border: 3px solid #ddd;
      border-radius: 50%;
      border-top-color: #0066cc;
      border-right-color: #0066cc;
    }

    .spinner--small .spinner__ring {
      left: 2px;
      width: 20px;
      height: 20px;
      border-width: 2px;
    }

    .spinner--medium .spinner__ring {
      left: 4px;
      width: 32px;
      height: 32px;
      border-width: 3px;
    }

    .spinner--large .spinner__ring {
      left: 6px;
      width: 44px;
      height: 44px;
      border-width: 4px;
    }

    .spinner__ring:nth-child(1) {
      animation-delay: -0.45s;
    }

    .spinner__ring:nth-child(2) {
      animation-delay: -0.3s;
    }

    @keyframes spinner-ring {
      0% {
        transform: rotate(0deg);
      }
      100% {
        transform: rotate(360deg);
      }
    }
  `]
})
export class SpinnerComponent {
  readonly size = input<SpinnerSize>('medium');
  readonly label = input<string>('Loading');
}
