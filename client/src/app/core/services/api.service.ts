import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  get<T>(url: string) {
    return this.http.get<T>(url);
  }

  post<T>(url: string, body: unknown) {
    return this.http.post<T>(url, body);
  }
}