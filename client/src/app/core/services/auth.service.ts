import { inject, Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { firstValueFrom } from 'rxjs';
import { UserRole } from '../models/user.model';

export interface LoginResponse {
  token: string;
  userId: string;
  email: string;
  fullName: string;
  role: string; // "Employee" | "Agent" | "Admin"
}

export interface CurrentUser {
  id: string;
  email: string;
  fullName: string;
  role: UserRole;
}

const TOKEN_KEY = 'helpdesk.token';
const USER_KEY = 'helpdesk.user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _user = signal<CurrentUser | null>(this.loadUserFromStorage());
  readonly user = this._user.asReadonly();

  readonly isAuthenticated = computed(() => this._user() !== null);

  get token(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  async login(email: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>('/api/auth/login', { email, password }),
    );

    localStorage.setItem(TOKEN_KEY, response.token);
    const user: CurrentUser = {
      id: response.userId,
      email: response.email,
      fullName: response.fullName,
      role: stringToRole(response.role),
    };
    localStorage.setItem(USER_KEY, JSON.stringify(user));
    this._user.set(user);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
    this.router.navigateByUrl('/login');
  }

  isInRole(role: UserRole): boolean {
    return this._user()?.role === role;
  }

  private loadUserFromStorage(): CurrentUser | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }
}

function stringToRole(role: string): UserRole {
  switch (role) {
    case 'Admin': return UserRole.Admin;
    case 'Agent': return UserRole.Agent;
    default: return UserRole.Employee;
  }
}