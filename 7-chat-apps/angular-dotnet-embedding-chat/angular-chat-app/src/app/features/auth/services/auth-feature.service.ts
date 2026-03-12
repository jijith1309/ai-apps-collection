import { Injectable, inject, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { TokenService } from '../../../core/services/token.service';
import { LoginRequest, LoginResponse } from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthFeatureService {
  private api = inject(ApiService);
  private tokenService = inject(TokenService);
  private router = inject(Router);

  loading = signal(false);
  error = signal<string | null>(null);
  isLoggedIn = signal(this.tokenService.isTokenValid());
  currentUser = signal(this.tokenService.getUser());

  async login(request: LoginRequest): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const res = await firstValueFrom(this.api.post<LoginResponse>('auth/login', request));
      if (res.success && res.data) {
        this.tokenService.saveToken(res.data.token);
        this.tokenService.saveUser({
          userId: res.data.userId,
          email: res.data.email,
          expiresAt: res.data.expiresAt
        });
        this.isLoggedIn.set(true);
        this.currentUser.set(this.tokenService.getUser());
        await this.router.navigate(['/documents']);
      } else {
        this.error.set(res.message ?? 'Login failed.');
      }
    } catch {
      this.error.set('Network error. Please try again.');
    } finally {
      this.loading.set(false);
    }
  }

  async register(request: LoginRequest): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const res = await firstValueFrom(this.api.post<LoginResponse>('auth/register', request));
      if (res.success && res.data) {
        this.tokenService.saveToken(res.data.token);
        this.tokenService.saveUser({
          userId: res.data.userId,
          email: res.data.email,
          expiresAt: res.data.expiresAt
        });
        this.isLoggedIn.set(true);
        this.currentUser.set(this.tokenService.getUser());
        await this.router.navigate(['/documents']);
      } else {
        this.error.set(res.message ?? 'Registration failed.');
      }
    } catch {
      this.error.set('Network error. Please try again.');
    } finally {
      this.loading.set(false);
    }
  }

  logout(): void {
    this.tokenService.clear();
    this.isLoggedIn.set(false);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }
}
