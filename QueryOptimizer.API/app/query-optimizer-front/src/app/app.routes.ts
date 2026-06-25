import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Analyzer } from './pages/analyzer/analyzer';
import { History } from './pages/history/history';
import { authGuard } from './core/auth.guard';
import { AnalysisDetails } from './pages/analysis-details/analysis-details';

export const routes: Routes = [
  { path: '', redirectTo: 'analyze', pathMatch: 'full' },
  { path: 'login', component: Login },
  { path: 'register', component: Register },
  { path: 'analyze', component: Analyzer, canActivate: [authGuard] },
  { path: 'history', component: History, canActivate: [authGuard] },
  { path: 'history/:analysisRunId', component: AnalysisDetails, canActivate: [authGuard] },
  { path: '**', redirectTo: 'analyze' }
];
