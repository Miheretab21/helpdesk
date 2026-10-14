import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly email = signal('bob@example.com');
  protected readonly password = signal('password123');
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(): Promise<void> {
    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.auth.login(this.email().trim(), this.password());
      await this.router.navigateByUrl('/');
    } catch {
      this.error.set('Invalid email or password.');
    } finally {
      this.submitting.set(false);
    }
  }
}