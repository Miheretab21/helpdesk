import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" [attr.aria-live]="'polite'" [attr.aria-atomic]="'true'">
      @for (toast of toastService.toasts$(); track toast.id) {
        <div [class]="'toast toast--' + toast.type" role="alert">
          <div class="toast__content">
            @switch(toast.type) {
              @case ('error') {
                <span class="toast__icon">⚠️</span>
              }
              @case ('success') {
                <span class="toast__icon">✓</span>
              }
              @case ('info') {
                <span class="toast__icon">ℹ️</span>
              }
            }
            <p class="toast__message">{{ toast.message }}</p>
          </div>
          <button
            type="button"
            class="toast__close"
            (click)="toastService.remove(toast.id)"
            aria-label="Close notification">
            ✕
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      bottom: 20px;
      right: 20px;
      z-index: 9999;
      display: flex;
      flex-direction: column;
      gap: 10px;
      pointer-events: none;
      max-width: 400px;
    }

    .toast {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 12px;
      padding: 12px 16px;
      border-radius: 4px;
      font-size: 14px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
      animation: toast-slide-in 0.3s ease-out;
      pointer-events: auto;
      background: white;
      border-left: 4px solid #ccc;
    }

    .toast--error {
      background: #fee;
      border-left-color: #d00;
      color: #700;
    }

    .toast--success {
      background: #efe;
      border-left-color: #070;
      color: #070;
    }

    .toast--info {
      background: #eef;
      border-left-color: #00d;
      color: #007;
    }

    .toast__content {
      display: flex;
      align-items: flex-start;
      gap: 10px;
      flex: 1;
    }

    .toast__icon {
      flex-shrink: 0;
      font-size: 18px;
    }

    .toast__message {
      margin: 0;
      padding: 0;
      line-height: 1.4;
    }

    .toast__close {
      flex-shrink: 0;
      background: none;
      border: none;
      font-size: 16px;
      cursor: pointer;
      color: inherit;
      opacity: 0.6;
      padding: 0;
      width: 24px;
      height: 24px;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: opacity 0.2s;
    }

    .toast__close:hover {
      opacity: 1;
    }

    @keyframes toast-slide-in {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }

    @media (max-width: 640px) {
      .toast-container {
        left: 10px;
        right: 10px;
        bottom: 10px;
        max-width: none;
      }
    }
  `]
})
export class ToastContainerComponent {
  protected readonly toastService = inject(ToastService);
}
