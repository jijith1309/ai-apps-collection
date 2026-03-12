import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { TopbarComponent } from './topbar.component';
import { SidebarComponent } from './sidebar.component';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [RouterOutlet, TopbarComponent, SidebarComponent],
  template: `
    <div class="min-h-screen flex flex-col" style="background: var(--p-surface-ground)">

      <!-- Topbar (fixed height, full width) -->
      <app-topbar (menuToggle)="sidebarVisible.set(!sidebarVisible())" />

      <div class="flex flex-1 relative">

        <!-- Sidebar -->
        <app-sidebar [visible]="sidebarVisible()" (close)="sidebarVisible.set(false)" />

        <!-- Desktop spacer: pushes content right when sidebar is visible -->
        <div class="hidden lg:block flex-shrink-0" style="width: 250px"></div>

        <!-- Main content -->
        <main class="flex-1 overflow-y-auto min-w-0">
          <router-outlet />
        </main>
      </div>
    </div>
  `
})
export class ShellComponent {
  sidebarVisible = signal(true);
}
