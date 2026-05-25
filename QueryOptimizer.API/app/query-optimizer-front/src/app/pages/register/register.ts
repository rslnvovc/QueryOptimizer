import { Component } from '@angular/core';
import { QueryOptimizationService } from '../../core/queryOptimization.service';
import { AuthService } from '../../core/auth.service';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  userName = '';
  loading = false;
  error = '';

  constructor(
    private readonly apiService: QueryOptimizationService,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  register(): void {
    if (!this.userName.trim()) {
      return;
    }

    this.loading = true;
    this.error = '';

    this.authService.register(this.userName.trim()).subscribe({
      next: user => {
        this.authService.setUser(user);
        this.router.navigate(['/analyze']);
      },
      error: err => {
        this.error = err?.error?.message ?? 'Registration failed.';
        this.loading = false;
      }
    });
  }
}