import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: string;
  type: 'error' | 'success' | 'info';
  message: string;
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly toasts = signal<Toast[]>([]);
  private nextId = 0;

  readonly toasts$ = this.toasts.asReadonly();

  error(message: string, duration = 4000) {
    this.add('error', message, duration);
  }

  success(message: string, duration = 3000) {
    this.add('success', message, duration);
  }

  info(message: string, duration = 3000) {
    this.add('info', message, duration);
  }

  private add(type: Toast['type'], message: string, duration: number) {
    const id = `toast-${++this.nextId}`;
    const toast: Toast = { id, type, message, duration };
    this.toasts.update(toasts => [...toasts, toast]);

    if (duration > 0) {
      setTimeout(() => this.remove(id), duration);
    }
  }

  remove(id: string) {
    this.toasts.update(toasts => toasts.filter(t => t.id !== id));
  }
}
