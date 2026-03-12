import { Component, inject, output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { AuthFeatureService } from '../../../features/auth/services/auth-feature.service';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [ButtonModule],
  template: `
    <div class="flex items-center justify-between px-4 h-16 shadow-sm"
         style="background: var(--p-surface-card); border-bottom: 1px solid var(--p-surface-border)">

      <!-- Left: hamburger + logo -->
      <div class="flex items-center gap-3">
        <p-button icon="pi pi-bars" variant="text" severity="secondary"
          size="small" (onClick)="menuToggle.emit()" />
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 rounded-lg flex items-center justify-center shadow-sm"
               style="background: var(--p-primary-color)">
            <i class="pi pi-comments text-sm" style="color: white"></i>
          </div>
          <span class="font-bold text-color text-base">DocChat</span>
        </div>
      </div>

      <!-- Right: user + sign out -->
      <div class="flex items-center gap-3">
        <div class="flex items-center gap-2">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold shadow-sm"
               style="background: var(--p-primary-color); color: white">
            {{ auth.currentUser()?.email?.charAt(0)?.toUpperCase() }}
          </div>
          <span class="text-sm text-muted-color hidden sm:block">{{ auth.currentUser()?.email }}</span>
        </div>
        <p-button label="Sign out" icon="pi pi-sign-out"
          severity="secondary" size="small" variant="text"
          (onClick)="auth.logout()" />
      </div>
    </div>
  `
})
export class TopbarComponent {
  auth = inject(AuthFeatureService);
  menuToggle = output<void>();
}
