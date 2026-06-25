import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { HistoryService } from '../../core/history.service';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-analysis-details',
  imports: [CommonModule, DatePipe],
  templateUrl: './analysis-details.html',
  styleUrl: './analysis-details.css',
})
export class AnalysisDetails implements OnInit {
  analysisRunId!: number;

  details: any = null;
  loading = false;
  error = '';

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly historyService: HistoryService,
    private readonly authService: AuthService,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('analysisRunId'));

    if (!id || Number.isNaN(id)) {
      this.error = 'Некоректний ID аналізу.';
      return;
    }

    this.analysisRunId = id;
    this.loadDetails();
  }

  loadDetails(): void {
    this.loading = true;
    this.error = '';

    this.historyService.getAnalysisDetails(this.analysisRunId).subscribe({
      next: result => {
        this.details = result;
        this.loading = false;
        this.changeDetectorRef.detectChanges();
      },
      error: err => {
        this.error = err?.error?.message ?? 'Не вдалося завантажити деталі аналізу.';
        this.loading = false;
        this.changeDetectorRef.detectChanges();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/history']);
  }

  goToAnalyzer(): void {
    this.router.navigate(['/analyze']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  get run(): any {
    return this.details?.analysisRun ?? this.details?.run ?? this.details;
  }

  get metrics(): any {
    return this.details?.originalMetrics
      ?? this.details?.metrics
      ?? this.details?.queryPerformanceMetrics
      ?? null;
  }

  get findings(): any[] {
    return this.details?.findings
      ?? this.details?.optimizationFindings
      ?? [];
  }

  get candidates(): any[] {
    return this.details?.candidateResults
      ?? this.details?.optimizationCandidates
      ?? this.details?.candidates
      ?? [];
  }

  get normalizedPlan(): any {
    return this.details?.normalizedPlan ?? this.details?.plan ?? null;
  }

  getProviderName(provider: number | string | null | undefined): string {
    switch (provider) {
      case 1:
      case '1':
      case 'SqlServer':
      case 'SQL Server':
        return 'SQL Server';

      case 2:
      case '2':
      case 'PostgreSql':
      case 'PostgreSQL':
        return 'PostgreSQL';

      case 3:
      case '3':
      case 'Oracle':
        return 'Oracle';

      default:
        return 'Н/Д';
    }
  }

  getPlanFormat(format: number | string | null | undefined): string {
    switch (format) {
      case 1:
      case '1':
      case 'Text':
        return 'Text';

      case 2:
      case '2':
      case 'Xml':
      case 'XML':
        return 'XML';

      case 3:
      case '3':
      case 'Json':
      case 'JSON':
        return 'JSON';

      default:
        return 'Н/Д';
    }
  }

  hasValue(value: any): boolean {
    return value !== null && value !== undefined && value !== '' && value !== '-';
  }

  formatValue(value: any, suffix = ''): string {
    if (!this.hasValue(value)) {
      return 'Н/Д';
    }

    return suffix ? `${value} ${suffix}` : `${value}`;
  }

  getMetricTiles(metrics: any) {
    if (!metrics) {
      return [];
    }

    return [
      { title: 'Провайдер', value: this.getProviderName(metrics.provider) },
      { title: 'Час виконання на клієнті', value: metrics.clientElapsedMs, suffix: 'ms' },
      { title: 'Час виконання в БД', value: metrics.databaseElapsedMs, suffix: 'ms' },
      { title: 'Процесорний час', value: metrics.cpuTimeMs, suffix: 'ms' },
      { title: 'Логічні читання', value: metrics.logicalReads },
      { title: 'Фізичні читання', value: metrics.physicalReads },
      { title: 'Повернуто записів', value: metrics.rowsReturned },
      { title: 'Змінено рядків', value: metrics.rowsAffected },
      { title: 'Оцінена вартість', value: metrics.estimatedCost },
      { title: 'Час планування', value: metrics.planningTimeMs, suffix: 'ms' },
      { title: 'Формат плану виконання', value: this.getPlanFormat(metrics.executionPlanFormat) }
    ].filter(tile => this.hasValue(tile.value));
  }

  formatJson(value: any): string {
    if (!value) {
      return '';
    }

    return JSON.stringify(value, null, 2);
  }
}
