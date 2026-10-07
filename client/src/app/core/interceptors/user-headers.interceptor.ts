import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CurrentUserService } from '../services/current-user.service';

export const userHeadersInterceptor: HttpInterceptorFn = (req, next) => {
  const currentUser = inject(CurrentUserService).user();

  if (!currentUser) {
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: {
      'X-User-Id': currentUser.id,
      'X-User-Role': roleToString(currentUser.role),
    },
  });

  return next(cloned);
};

function roleToString(role: number): string {
  // Backend expects the enum name, e.g. "Employee", "Agent", "Admin".
  switch (role) {
    case 1: return 'Employee';
    case 2: return 'Agent';
    case 3: return 'Admin';
    default: return 'Employee';
  }
}