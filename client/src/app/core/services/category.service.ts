import { inject, Injectable } from '@angular/core';
import { httpResource } from '@angular/common/http';
import { Category } from '../models/ticket.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  readonly all = httpResource<Category[]>(
    () => '/api/categories',
    { defaultValue: [] },
  );
}