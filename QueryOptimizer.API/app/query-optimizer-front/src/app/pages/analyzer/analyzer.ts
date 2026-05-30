import { ChangeDetectorRef, Component } from '@angular/core';
import { DatabaseTypes } from '../../models/databaseType';
import { QueryOptimizationResult } from '../../models/queryOptimizationResult';
import { QueryOptimizationService } from '../../core/queryOptimization.service';
import { AuthService } from '../../core/auth.service';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { ConnectionStringModel } from '../../models/connectionStringModel';

@Component({
  selector: 'app-analyzer',
  imports: [CommonModule, FormsModule],
  templateUrl: './analyzer.html',
  styleUrl: './analyzer.css',
})
export class Analyzer {
  DatabaseTypes = DatabaseTypes;

  provider = DatabaseTypes.SqlServer;

  connectionStringModel: ConnectionStringModel = {
    provider: DatabaseTypes.SqlServer,
    serverName: 'localhost\\SQLEXPRESS',
    databaseName: 'Northwind',
    userName: null,
    password: null,
    host: null,
    port: null,
    serviceName: null
  };

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

  onProviderChange(provider: DatabaseTypes): void {
    this.provider = provider;

    if (provider === DatabaseTypes.SqlServer) {
      this.connectionStringModel = {
        provider,
        serverName: 'localhost\\SQLEXPRESS',
        databaseName: 'Northwind',
        userName: null,
        password: null,
        host: null,
        port: null,
        serviceName: null
      };

      return;
    }

    if (provider === DatabaseTypes.PostgreSql) {
      this.connectionStringModel = {
        provider,
        serverName: 'PostgreSQL',
        host: 'localhost',
        port: 5432,
        databaseName: '',
        userName: '',
        password: '',
        serviceName: null
      };

      return;
    }

    if (provider === DatabaseTypes.Oracle) {
      this.connectionStringModel = {
        provider,
        serverName: null,
        databaseName: null,
        host: 'localhost',
        port: 1521,
        serviceName: 'orclpdb',
        userName: '',
        password: ''
      };
    }
  }

  isSqlServer(): boolean {
    return this.connectionStringModel.provider === DatabaseTypes.SqlServer;
  }

  isPostgreSql(): boolean {
    return this.connectionStringModel.provider === DatabaseTypes.PostgreSql;
  }

  isOracle(): boolean {
    return this.connectionStringModel.provider === DatabaseTypes.Oracle;
  }

  get isConnectionModelValid(): boolean {
    const model = this.connectionStringModel;

    if (model.provider === DatabaseTypes.SqlServer) {
      return this.hasValue(model.serverName) &&
             this.hasValue(model.databaseName);
    }

    if (model.provider === DatabaseTypes.PostgreSql) {
      return this.hasValue(model.serverName) &&
             this.hasValue(model.databaseName) &&
             this.hasValue(model.userName) &&
             this.hasValue(model.password) &&
             this.hasValue(model.host) &&
             model.port != null;
    }

    if (model.provider === DatabaseTypes.Oracle) {
      return this.hasValue(model.userName) &&
             this.hasValue(model.password) &&
             this.hasValue(model.host) &&
             model.port != null &&
             this.hasValue(model.serviceName);
    }

    return false;
  }

  analyze(): void {
    const user = this.authService.getUser();

    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    if (!this.sql.trim()) {
      this.error = 'SQL query is required.';
      return;
    }

    if (!this.isConnectionModelValid) {
      this.error = 'Please fill all required database connection fields.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.result = null;

    this.apiService.analyze({
      userId: user.id,
      connectionStringModel: this.connectionStringModel,
      sql: this.sql,
      //parameters: {}
    })
    .pipe(
      finalize(() => {
        this.loading = false;
        this.changeDetectorRef.detectChanges();
      })
    )
    .subscribe({
      next: result => {
        this.result = result;
        this.changeDetectorRef.detectChanges();
      },
      error: err => {
        this.error = err?.error?.message ?? 'Query analysis failed.';
        this.changeDetectorRef.detectChanges();
      }
    });
  }

  goToHistory(): void {
    this.router.navigate(['/history']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private hasValue(value?: string | null): boolean {
    return value !== null && value !== undefined && value.trim().length > 0;
  }
}