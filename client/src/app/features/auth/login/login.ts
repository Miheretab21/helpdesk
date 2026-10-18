import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

const EMAIL_PATTERN = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

const DEMO_PASSWORD = 'password123';

type LoginField = 'email' | 'password';

interface DemoAccount {
  readonly name: string;
  readonly email: string;
  readonly role: string;
}

@Component({
  selector: 'app-login',
  imports: [FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly demoAccounts: readonly DemoAccount[] = [
    { name: 'Alice', email: 'alice@example.com', role: 'Employee' },
    { name: 'Bob', email: 'bob@example.com', role: 'Agent' },
    { name: 'Carol', email: 'carol@example.com', role: 'Admin' },
  ];

  protected readonly email = signal('');
  protected readonly password = signal('');
  protected readonly touched = signal<Record<LoginField, boolean>>({ email: false, password: false });
  protected readonly submitting = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly emailInvalid = computed(() => {
    const value = this.email().trim();
    return value.length === 0 || !EMAIL_PATTERN.test(value);
  });

  protected readonly passwordInvalid = computed(() => this.password().length === 0);

  // Messages are only surfaced once a field has been blurred, so the form does
  // not shout at the user while they are still filling it in.
  protected readonly emailError = computed(() => {
    if (!this.touched().email) return null;
    return this.emailInvalid() ? 'Enter a valid email address.' : null;
  });

  protected readonly passwordError = computed(() => {
    if (!this.touched().password) return null;
    return this.passwordInvalid() ? 'Password is required.' : null;
  });

  protected readonly canSubmit = computed(
    () => !this.emailInvalid() && !this.passwordInvalid() && !this.submitting(),
  );

  protected markTouched(field: LoginField): void {
    this.touched.update(state => ({ ...state, [field]: true }));
  }

  // Fills the form with a seeded demo account and signs in immediately so a
  // reviewer can jump into any role in a single click.
  protected useDemoAccount(account: DemoAccount): void {
    if (this.submitting()) return;
    this.email.set(account.email);
    this.password.set(DEMO_PASSWORD);
    void this.submit();
  }

  protected async submit(): Promise<void> {
    this.touched.set({ email: true, password: true });
    if (!this.canSubmit()) return;

    this.submitting.set(true);
    this.error.set(null);

    try {
      await this.auth.login(this.email().trim(), this.password());
      await this.router.navigateByUrl('/');
    } catch {
      this.error.set('Invalid email or password. Please try again.');
    } finally {
      this.submitting.set(false);
    }
  }
}
