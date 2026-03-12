import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { AuthFeatureService } from '../services/auth-feature.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule, RouterLink, ButtonModule, CardModule, InputTextModule, PasswordModule, MessageModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-surface-ground px-4 relative overflow-hidden">
      <div class="absolute -top-32 -right-32 w-96 h-96 rounded-full blur-3xl opacity-20"
           style="background: var(--p-primary-color)"></div>
      <div class="absolute -bottom-32 -left-32 w-96 h-96 rounded-full blur-3xl opacity-10"
           style="background: var(--p-primary-color)"></div>

      <div class="w-full max-w-sm relative z-10">
        <div class="text-center mb-8">
          <a routerLink="/" class="no-underline">
            <div class="inline-flex w-16 h-16 rounded-2xl items-center justify-center mb-4 shadow-lg"
                 style="background: var(--p-primary-color)">
              <i class="pi pi-comments text-3xl" style="color:white"></i>
            </div>
            <h1 class="text-3xl font-bold text-color">DocChat</h1>
          </a>
          <p class="text-muted-color text-sm mt-1">AI-powered document conversations</p>
        </div>

        <p-card>
          <div class="flex flex-col gap-5">
            <div class="text-center">
              <h2 class="text-xl font-semibold text-color">
                {{ isRegisterMode() ? 'Create account' : 'Welcome back' }}
              </h2>
              <p class="text-muted-color text-sm mt-1">
                {{ isRegisterMode() ? 'Start chatting with your docs' : 'Sign in to continue' }}
              </p>
            </div>

            @if (auth.error()) {
              <p-message severity="error" [text]="auth.error()!" />
            }

            <div class="flex flex-col gap-1">
              <label class="text-sm font-medium text-color">Email</label>
              <input pInputText type="email" [(ngModel)]="email"
                placeholder="you@example.com" class="w-full" />
            </div>

            <div class="flex flex-col gap-1">
              <label class="text-sm font-medium text-color">Password</label>
              <p-password [(ngModel)]="password" [feedback]="isRegisterMode()"
                [toggleMask]="true" class="w-full" inputStyleClass="w-full" />
            </div>

            <p-button
              [label]="isRegisterMode() ? 'Create Account' : 'Sign In'"
              [icon]="isRegisterMode() ? 'pi pi-user-plus' : 'pi pi-sign-in'"
              [loading]="auth.loading()"
              (onClick)="submit()"
              [fluid]="true"
            />

            <p class="text-center text-sm text-muted-color m-0">
              {{ isRegisterMode() ? 'Already have an account?' : "Don't have an account?" }}
              <a class="text-primary cursor-pointer font-semibold ml-1 hover:underline"
                (click)="isRegisterMode.set(!isRegisterMode())">
                {{ isRegisterMode() ? 'Sign in' : 'Register' }}
              </a>
            </p>
          </div>
        </p-card>
      </div>
    </div>
  `
})
export class LoginComponent implements OnInit {
  auth = inject(AuthFeatureService);
  private route = inject(ActivatedRoute);

  isRegisterMode = signal(false);
  email = '';
  password = '';

  ngOnInit(): void {
    const mode = this.route.snapshot.queryParamMap.get('mode');
    if (mode === 'register') this.isRegisterMode.set(true);
  }

  async submit(): Promise<void> {
    const request = { email: this.email, password: this.password };
    if (this.isRegisterMode()) {
      await this.auth.register(request);
    } else {
      await this.auth.login(request);
    }
  }
}
