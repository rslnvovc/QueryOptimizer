import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router } from '@angular/router';
import { QueryOptimizationService } from '../../core/queryOptimization.service';
import { AuthService } from '../../core/auth.service';
import { QueryAnalysisRun } from '../../models/queryAnalysisRun';
import { HistoryService } from '../../core/history.service';

@Component({
  selector: 'app-history',
  imports: [CommonModule, DatePipe],
  templateUrl: './history.html',
  styleUrl: './history.css'
})
export class History implements OnInit {
  history: QueryAnalysisRun[] = [];

  loading = false;
  error = '';

  constructor(
    private readonly queryOptimizationService: QueryOptimizationService,
    private readonly historyService: HistoryService,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    const user = this.authService.getUser();

    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    this.loading = true;
    this.error = '';

    this.historyService.getUserHistory(user.id).subscribe({
      next: result => {
        this.history = result;
        this.loading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: err => {
        this.error = err?.error?.message ?? 'Failed to load history.';
        this.loading = false;
        this.changeDetectorRef.detectChanges()
      }
    });
  }

  openDetails(run: QueryAnalysisRun): void {
    this.router.navigate(['/history', run.id]);
  }

  goToAnalyzer(): void {
    this.router.navigate(['/analyze']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  getProviderName(provider: number): string {
    switch (provider) {
      case 1:
        return 'SQL Server';
      case 2:
        return 'PostgreSQL';
      case 3:
        return 'Oracle';
      default:
        return 'Unknown';
    }
  }

  getStatusClass(status: string): string {
    switch (status?.toLowerCase()) {
      case 'completed':
        return 'bg-green-100 text-green-700';
      case 'failed':
        return 'bg-red-100 text-red-700';
      case 'running':
        return 'bg-yellow-100 text-yellow-700';
      default:
        return 'bg-slate-100 text-slate-700';
    }
  }
}