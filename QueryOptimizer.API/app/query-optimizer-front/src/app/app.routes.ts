import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Analyzer } from './pages/analyzer/analyzer';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'analyze', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'analyze', component: Analyzer, canActivate: [authGuard] },
  { path: '**', redirectTo: 'analyze' }
];
