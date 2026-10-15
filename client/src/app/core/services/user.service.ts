import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserRole } from '../models/user.model';

export interface UserListItem {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
}

export interface UserDetail {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  createdAt: string;
}

export interface CreateUserRequest {
  email: string;
  fullName: string;
  password: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  fullName?: string;
  role?: UserRole;
  password?: string;
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/users';

  list() {
    return this.http.get<UserListItem[]>(this.baseUrl);
  }

  getById(id: string) {
    return this.http.get<UserDetail>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateUserRequest) {
    return this.http.post<string>(this.baseUrl, request);
  }

  update(id: string, request: UpdateUserRequest) {
    return this.http.put(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string) {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }
}
