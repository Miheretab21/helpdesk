import { Component, inject } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { AuthService } from '../../../core/services/auth.service';
import { TicketStatus } from '../../../core/models/ticket.model';

export interface AgentDashboardDto {
  myTicketsByStatus: Record<number, number>;
  unassignedQueueCount: number;
  myRecentActivity: any[];
}

@Component({
  selector: 'app-agent-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SpinnerComponent],
  template: `
    <div class="dashboard agent-dashboard">
      <header class="dashboard__header">
        <h1>My Work</h1>
      </header>

      @if (dashboard.isLoading()) {
        <div class="dashboard__loading">
          <app-spinner size="large" />
          <p>Loading your dashboard…</p>
        </div>
      } @else if (dashboard.error()) {
        <p class="dashboard__error">Could not load dashboard.</p>
      } @else if (dashboard.value(); as data) {
        <div class="dashboard__two-column">
          <section class="dashboard__section">
            <h2>My Assigned Tickets</h2>
            <div class="dashboard__stats">
              <div class="stat-card" [routerLink]="'/tickets'" [queryParams]="{ status: TicketStatus.Assigned }">
                <div class="stat-card__value">{{ data.myTicketsByStatus[TicketStatus.Assigned] || 0 }}</div>
                <div class="stat-card__label">Assigned</div>
              </div>
              <div class="stat-card" [routerLink]="'/tickets'" [queryParams]="{ status: TicketStatus.InProgress }">
                <div class="stat-card__value">{{ data.myTicketsByStatus[TicketStatus.InProgress] || 0 }}</div>
                <div class="stat-card__label">In Progress</div>
              </div>
              <div class="stat-card" [routerLink]="'/tickets'" [queryParams]="{ status: TicketStatus.Resolved }">
                <div class="stat-card__value">{{ data.myTicketsByStatus[TicketStatus.Resolved] || 0 }}</div>
                <div class="stat-card__label">Resolved</div>
              </div>
            </div>
          </section>

          <section class="dashboard__section">
            <h2>Your Inbox</h2>
            <div class="dashboard__inbox">
              <div class="inbox-card" [routerLink]="'/tickets'" [queryParams]="{ status: TicketStatus.New }">
                <div class="inbox-card__value">{{ data.unassignedQueueCount }}</div>
                <div class="inbox-card__label">Unassigned Tickets</div>
                <div class="inbox-card__action">Claim »</div>
              </div>
            </div>
          </section>
        </div>

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
          <a routerLink="/tickets" class="btn btn--secondary">View All Tickets</a>
        </section>
      }
    </div>
  `,
  styles: [`
    .agent-dashboard {
      .dashboard__header {
        margin-bottom: 30px;

        h1 {
          margin: 0;
        }
      }

      .dashboard__two-column {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 20px;
        margin-bottom: 20px;

        @media (max-width: 768px) {
          grid-template-columns: 1fr;
        }
      }

      .dashboard__stats {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
        gap: 12px;
      }

      .stat-card {
        cursor: pointer;
        transition: transform 0.2s, box-shadow 0.2s;

        &:hover {
          transform: translateY(-2px);
          box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        }
      }

      .dashboard__inbox {
        display: grid;
        gap: 12px;
      }

      .inbox-card {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        padding: 24px;
        border-radius: 8px;
        cursor: pointer;
        transition: transform 0.2s;
        text-decoration: none;

        &:hover {
          transform: translateY(-2px);
        }
      }

      .inbox-card__value {
        font-size: 36px;
        font-weight: 700;
        margin-bottom: 4px;
      }

      .inbox-card__label {
        font-size: 14px;
        opacity: 0.9;
        margin-bottom: 12px;
      }

      .inbox-card__action {
        font-size: 12px;
        opacity: 0.8;
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
export class AgentDashboardComponent {
  protected readonly TicketStatus = TicketStatus;
  protected readonly auth = inject(AuthService);

  protected readonly dashboard = httpResource<AgentDashboardDto>(
    () => '/api/dashboard/agent'
  );

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }
}
