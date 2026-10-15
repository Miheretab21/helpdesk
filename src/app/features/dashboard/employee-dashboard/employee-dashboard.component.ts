import { Component, inject, computed } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { AuthService } from '../../../core/services/auth.service';
import { TicketStatus, TicketPriority } from '../../../core/models/ticket.model';

export interface EmployeeDashboardDto {
  myTicketsByStatus: Record<number, number>;
  myRecentActivity: any[];
}

@Component({
  selector: 'app-employee-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SpinnerComponent],
  template: `
    <div class="dashboard employee-dashboard">
      <header class="dashboard__header">
        <h1>My Tickets</h1>
        <a routerLink="/tickets/new" class="btn btn--primary">Create New Ticket</a>
      </header>

      @if (dashboard.isLoading()) {
        <div class="dashboard__loading">
          <app-spinner size="large" />
          <p>Loading your dashboard…</p>
        </div>
      } @else if (dashboard.error()) {
        <p class="dashboard__error">Could not load dashboard.</p>
      } @else if (dashboard.value(); as data) {
        <section class="dashboard__section">
          <h2>My Tickets by Status</h2>
          <div class="dashboard__stats">
            <div class="stat-card" [routerLink]="'/tickets'" [queryParams]="{ status: TicketStatus.New }">
              <div class="stat-card__value">{{ data.myTicketsByStatus[TicketStatus.New] || 0 }}</div>
              <div class="stat-card__label">New</div>
            </div>
            <div class="stat-card" [routerLink]="'/tickets'" [queryParams]="{ status: TicketStatus.InProgress }">
              <div class="stat-card__value">{{ data.myTicketsByStatus[TicketStatus.InProgress] || 0 }}</div>
              <div class="stat-card__label">In Progress</div>
            </div>
            <div class="stat-card" [routerLink]="'/tickets'" [queryParams]="{ status: TicketStatus.Resolved }">
              <div class="stat-card__value">{{ data.myTicketsByStatus[TicketStatus.Resolved] || 0 }}</div>
              <div class="stat-card__label">Resolved</div>
            </div>
            <div class="stat-card" [routerLink]="'/tickets'" [queryParams]="{ status: TicketStatus.Closed }">
              <div class="stat-card__value">{{ data.myTicketsByStatus[TicketStatus.Closed] || 0 }}</div>
              <div class="stat-card__label">Closed</div>
            </div>
          </div>
        </section>

        @if (data.myRecentActivity.length > 0) {
          <section class="dashboard__section">
            <h2>Recent Activity</h2>
            <ul class="dashboard__activity">
              @for (activity of data.myRecentActivity; track activity.id) {
                <li>
                  <a [routerLink]="['/tickets', activity.id]" class="dashboard__activity-link">
                    <strong>{{ activity.title }}</strong>
                    <span class="badge badge--status">{{ statusLabel(activity.status) }}</span>
                  </a>
                </li>
              }
            </ul>
          </section>
        }

        <section class="dashboard__section">
          <a routerLink="/tickets" class="btn btn--secondary">View All My Tickets</a>
        </section>
      }
    </div>
  `,
  styles: [`
    .employee-dashboard {
      .dashboard__header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 30px;

        h1 {
          margin: 0;
        }
      }

      .dashboard__stats {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
        gap: 16px;
      }

      .stat-card {
        cursor: pointer;
        transition: transform 0.2s, box-shadow 0.2s;

        &:hover {
          transform: translateY(-2px);
          box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }
      }

      .dashboard__activity {
        list-style: none;
        padding: 0;
        margin: 0;
      }

      .dashboard__activity-link {
        display: block;
        padding: 12px 16px;
        border: 1px solid #e0e0e0;
        border-radius: 4px;
        text-decoration: none;
        color: #0066cc;
        margin-bottom: 8px;
        transition: background-color 0.2s;

        &:hover {
          background-color: #f5f5f5;
        }

        strong {
          display: block;
          color: #333;
          margin-bottom: 4px;
        }
      }
    }
  `]
})
export class EmployeeDashboardComponent {
  protected readonly TicketStatus = TicketStatus;
  protected readonly auth = inject(AuthService);

  protected readonly dashboard = httpResource<EmployeeDashboardDto>(
    () => '/api/dashboard/employee'
  );

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }
}
