import { Component, inject } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { SpinnerComponent } from '../../../shared/components/spinner/spinner.component';
import { AuthService } from '../../../core/services/auth.service';
import { TicketStatus } from '../../../core/models/ticket.model';

export interface AdminDashboardDto {
  allTicketsByStatus: Record<number, number>;
  ticketsByCategory: Record<string, number>;
  workloadByAgent: Record<string, number>;
  unassignedBacklog: number;
  averageTimeToClose: number;
  recentActivitySystem: any[];
  topAgentsThisWeek: Array<{ agentName: string; resolvedCount: number }>;
}

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, SpinnerComponent],
  template: `
    <div class="dashboard admin-dashboard">
      <header class="dashboard__header">
        <h1>Dashboard</h1>
      </header>

      @if (dashboard.isLoading()) {
        <div class="dashboard__loading">
          <app-spinner size="large" />
          <p>Loading dashboard…</p>
        </div>
      } @else if (dashboard.error()) {
        <p class="dashboard__error">Could not load dashboard.</p>
      } @else if (dashboard.value(); as data) {
        <section class="dashboard__section">
          <h2>Status Overview</h2>
          <div class="dashboard__stats">
            <div class="stat-card">
              <div class="stat-card__value">{{ data.allTicketsByStatus[TicketStatus.New] || 0 }}</div>
              <div class="stat-card__label">New</div>
            </div>
            <div class="stat-card">
              <div class="stat-card__value">{{ data.allTicketsByStatus[TicketStatus.Assigned] || 0 }}</div>
              <div class="stat-card__label">Assigned</div>
            </div>
            <div class="stat-card">
              <div class="stat-card__value">{{ data.allTicketsByStatus[TicketStatus.InProgress] || 0 }}</div>
              <div class="stat-card__label">In Progress</div>
            </div>
            <div class="stat-card">
              <div class="stat-card__value">{{ data.allTicketsByStatus[TicketStatus.Resolved] || 0 }}</div>
              <div class="stat-card__label">Resolved</div>
            </div>
            <div class="stat-card">
              <div class="stat-card__value">{{ data.allTicketsByStatus[TicketStatus.Closed] || 0 }}</div>
              <div class="stat-card__label">Closed</div>
            </div>
          </div>
        </section>

        <section class="dashboard__section">
          <h2>Key Metrics</h2>
          <div class="dashboard__metrics">
            <div class="metric-card">
              <div class="metric-card__icon">📊</div>
              <div class="metric-card__value">{{ data.unassignedBacklog }}</div>
              <div class="metric-card__label">Unassigned Backlog</div>
            </div>
            <div class="metric-card">
              <div class="metric-card__icon">⏱️</div>
              <div class="metric-card__value">{{ data.averageTimeToClose.toFixed(1) }}</div>
              <div class="metric-card__label">Avg Days to Close</div>
            </div>
            <div class="metric-card">
              <div class="metric-card__icon">👥</div>
              <div class="metric-card__value">{{ countActiveAgents(data) }}</div>
              <div class="metric-card__label">Active Agents</div>
            </div>
          </div>
        </section>

        <div class="dashboard__two-column">
          @if (Object.keys(data.workloadByAgent).length > 0) {
            <section class="dashboard__section">
              <h2>Agent Workload</h2>
              <ul class="dashboard__list">
                @for (agent of workloadList(data); track agent.name) {
                  <li class="list-item">
                    <span class="list-item__name">{{ agent.name }}</span>
                    <span class="list-item__value">{{ agent.count }} active</span>
                  </li>
                }
              </ul>
            </section>
          }

          @if (data.topAgentsThisWeek.length > 0) {
            <section class="dashboard__section">
              <h2>Top Agents (This Week)</h2>
              <ul class="dashboard__list">
                @for (agent of data.topAgentsThisWeek; track agent.agentName) {
                  <li class="list-item">
                    <span class="list-item__name">{{ agent.agentName }}</span>
                    <span class="list-item__value">{{ agent.resolvedCount }} resolved</span>
                  </li>
                }
              </ul>
            </section>
          }
        </div>

        @if (data.recentActivitySystem.length > 0) {
          <section class="dashboard__section">
            <h2>Recent Activity (System-wide)</h2>
            <ul class="dashboard__activity">
              @for (activity of data.recentActivitySystem; track activity.id) {
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
    .admin-dashboard {
      .dashboard__header {
        margin-bottom: 30px;

        h1 {
          margin: 0;
        }
      }

      .dashboard__stats {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
        gap: 12px;
      }

      .dashboard__metrics {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
        gap: 16px;
        margin-bottom: 20px;
      }

      .metric-card {
        background: white;
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        padding: 20px;
        text-align: center;
      }

      .metric-card__icon {
        font-size: 24px;
        margin-bottom: 8px;
      }

      .metric-card__value {
        font-size: 28px;
        font-weight: 700;
        color: #0066cc;
        margin-bottom: 4px;
      }

      .metric-card__label {
        font-size: 13px;
        color: #666;
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

      .dashboard__list {
        list-style: none;
        padding: 0;
        margin: 0;
      }

      .list-item {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 12px 16px;
        border-bottom: 1px solid #e0e0e0;

        &:last-child {
          border-bottom: none;
        }
      }

      .list-item__name {
        color: #333;
        font-weight: 500;
      }

      .list-item__value {
        font-size: 13px;
        color: #666;
        background: #f5f5f5;
        padding: 4px 8px;
        border-radius: 3px;
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
export class AdminDashboardComponent {
  protected readonly TicketStatus = TicketStatus;
  protected readonly auth = inject(AuthService);

  protected readonly dashboard = httpResource<AdminDashboardDto>(
    () => '/api/dashboard/admin'
  );

  protected statusLabel(status: TicketStatus): string {
    return TicketStatus[status];
  }

  protected workloadList(data: AdminDashboardDto) {
    return Object.entries(data.workloadByAgent)
      .map(([name, count]) => ({ name, count }))
      .sort((a, b) => b.count - a.count);
  }

  protected countActiveAgents(data: AdminDashboardDto) {
    return Object.values(data.workloadByAgent).filter(count => count > 0).length;
  }
}
