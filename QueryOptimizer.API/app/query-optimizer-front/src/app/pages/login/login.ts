import { Component } from '@angular/core';
import { AuthService } from '../../core/auth.service';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  userName = '';
  loading = false;
  error = '';

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {}

  login(): void {
    if (!this.userName.trim()) {
      return;
    }

    this.loading = true;
    this.error = '';

    this.authService.login(this.userName.trim()).subscribe({
      next: user => {
        this.authService.setUser(user);
        this.router.navigate(['/analyze']);
      },
      error: err => {
        this.error = err?.error?.message ?? 'Login failed.';
        this.loading = false;
      }
    });
  }
}