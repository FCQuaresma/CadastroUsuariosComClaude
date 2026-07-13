import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { roleGuard } from './core/auth/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'users',
    loadComponent: () => import('./features/users/user-list/user-list.component').then(m => m.UserListComponent),
    canActivate: [authGuard]
  },
  {
    path: 'users/new',
    loadComponent: () => import('./features/users/user-form/user-form.component').then(m => m.UserFormComponent),
    canActivate: [authGuard, roleGuard(['Admin'])]
  },
  {
    path: 'users/edit/:id',
    loadComponent: () => import('./features/users/user-form/user-form.component').then(m => m.UserFormComponent),
    canActivate: [authGuard, roleGuard(['Admin'])]
  },
  { path: '', redirectTo: 'users', pathMatch: 'full' },
  { path: '**', redirectTo: 'users' }
];
