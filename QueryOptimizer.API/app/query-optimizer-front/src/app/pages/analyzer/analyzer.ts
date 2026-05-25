import { ChangeDetectorRef, Component } from '@angular/core';
import { DatabaseTypes } from '../../models/databaseType';
import { QueryOptimizationResult } from '../../models/queryOptimizationResult';
import { QueryOptimizationService } from '../../core/queryOptimization.service';
import { AuthService } from '../../core/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-analyzer',
  imports: [CommonModule, FormsModule],
  templateUrl: './analyzer.html',
  styleUrl: './analyzer.css',
})
export class Analyzer {
  DatabaseTypes = DatabaseTypes;

  provider = DatabaseTypes.SqlServer;

  connectionString =
    'Data Source=localhost\\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;';

  sql = `SELECT ProductName, Total = SUM(Quantity)
FROM Products P, [Order Details] OD, Orders O, Customers C
WHERE C.CustomerID = O.CustomerID 
  AND O.OrderID = OD.OrderID 
  AND OD.ProductID = P.ProductID
GROUP BY ProductName`;

  loading = false;
  error = '';
  result: QueryOptimizationResult | null = null;

  get userName(): string {
    return this.authService.getUser()?.userName ?? '';
  }

  constructor(
    private readonly apiService: QueryOptimizationService,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly changeDetectorRef: ChangeDetectorRef
  ) {}

  analyze(): void {
    const user = this.authService.getUser();

    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.connectionString.trim() || !this.sql.trim()) {
      this.error = 'Connection string and SQL query are required.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.result = null;

    this.apiService.analyze({
  userId: user.id,
  provider: this.provider,
  connectionString: this.connectionString,
  sql: this.sql
})
.pipe(
  finalize(() => {
    this.loading = false;
    this.changeDetectorRef.detectChanges();
  })
)
.subscribe({
  next: result => {
    console.log('Analyze result:', result);

    this.result = result;

    console.log('Component result after assign:', this.result);

    this.changeDetectorRef.detectChanges();
  },
  error: err => {
    console.error('Analyze error:', err);

    this.error = err?.error?.message ?? 'Query analysis failed.';

    this.changeDetectorRef.detectChanges();
  }
});
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}