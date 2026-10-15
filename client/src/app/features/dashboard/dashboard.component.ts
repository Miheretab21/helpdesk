import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { UserRole } from '../../core/models/user.model';
import { EmployeeDashboardComponent } from './employee-dashboard/employee-dashboard.component';
import { AgentDashboardComponent } from './agent-dashboard/agent-dashboard.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    EmployeeDashboardComponent,
    AgentDashboardComponent,
    AdminDashboardComponent,
  ],
  template: `
    @switch (auth.user()?.role) {
      @case (UserRole.Admin) {
        <app-admin-dashboard />
      }
      @case (UserRole.Agent) {
        <app-agent-dashboard />
      }
      @default {
        <app-employee-dashboard />
      }
    }
  `,
})
export class DashboardComponent {
  protected readonly auth = inject(AuthService);
  protected readonly UserRole = UserRole;
}
