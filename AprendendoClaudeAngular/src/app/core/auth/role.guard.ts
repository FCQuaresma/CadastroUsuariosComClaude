import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export function roleGuard(allowedRoles: string[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const role = authService.getRole();

    if (role && allowedRoles.includes(role)) {
      return true;
    }

    router.navigateByUrl('/users');
    return false;
  };
}
