import { Component, inject } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UserRole } from '../../../core/models/user.model';
import { ToastContainerComponent } from '../../../shared/components/toast-container/toast-container.component';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, ToastContainerComponent],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.scss',
})

export class AppShell {
  protected readonly auth = inject(AuthService);
  protected readonly UserRole = UserRole;
}
