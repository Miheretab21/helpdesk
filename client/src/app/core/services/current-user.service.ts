import { Injectable, signal } from '@angular/core';
import { UserRole } from '../models/user.model';

export interface CurrentUser {
  id: string;
  role: UserRole;
  fullName: string;
}

const DEV_USER: CurrentUser = {
  id: '09406e38-cbcb-4747-be2e-d361a12df4fc',
  role: UserRole.Employee,
  fullName: 'Alice Employee',
};

@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  private readonly _user = signal<CurrentUser | null>(DEV_USER);
  readonly user = this._user.asReadonly();

  isInRole(role: UserRole): boolean {
    return this._user()?.role === role;
  }

  setUser(user: CurrentUser | null) {
    this._user.set(user);
  }
}