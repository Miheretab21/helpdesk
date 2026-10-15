import { inject, Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { User } from '../models/user.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly api = inject(ApiService);

  list() {
    return this.api.get<User[]>('/api/users');
  }

  getById(id: string) {
    return this.api.get<User>(`/api/users/${id}`);
  }
}
