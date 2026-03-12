import { Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ButtonModule } from 'primeng/button';

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, ButtonModule],
  template: `
    <!-- Overlay backdrop on mobile -->
    @if (visible()) {
      <div class="fixed inset-0 z-20 bg-black/30 lg:hidden"
           (click)="close.emit()"></div>
    }

    <!-- Sidebar panel -->
    <aside class="fixed top-0 left-0 h-full z-30 flex flex-col transition-transform duration-300 lg:translate-x-0"
           [class.translate-x-0]="visible()"
           [class.-translate-x-full]="!visible()"
           style="width: 250px; background: var(--p-surface-card); border-right: 1px solid var(--p-surface-border)">

      <!-- Logo -->
      <div class="flex items-center gap-3 px-5 h-16"
           style="border-bottom: 1px solid var(--p-surface-border)">
        <div class="w-9 h-9 rounded-xl flex items-center justify-center shadow-sm"
             style="background: var(--p-primary-color)">
          <i class="pi pi-comments text-sm" style="color: white"></i>
        </div>
        <div>
          <p class="font-bold text-color text-base m-0 leading-tight">DocChat</p>
          <p class="text-xs text-muted-color m-0">AI Document Chat</p>
        </div>
      </div>

      <!-- Navigation -->
      <nav class="flex-1 px-3 py-4 flex flex-col gap-1 overflow-y-auto">
        <p class="text-xs font-semibold text-muted-color uppercase tracking-widest px-3 mb-2">Menu</p>
        @for (item of navItems; track item.route) {
          <a [routerLink]="item.route" routerLinkActive="sidebar-active"
             class="sidebar-link flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-muted-color no-underline transition-all"
             (click)="close.emit()">
            <i [class]="'pi ' + item.icon + ' text-base'"></i>
            <span>{{ item.label }}</span>
          </a>
        }
      </nav>

      <!-- Footer -->
      <div class="px-4 py-4" style="border-top: 1px solid var(--p-surface-border)">
        <p class="text-xs text-muted-color text-center m-0">DocChat &copy; 2025</p>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar-link:hover { background: var(--p-surface-hover); color: var(--p-text-color); }
    :host ::ng-deep .sidebar-active {
      background: var(--p-primary-50) !important;
      color: var(--p-primary-color) !important;
      font-weight: 600;
    }
    :host ::ng-deep .sidebar-active i { color: var(--p-primary-color) !important; }
  `]
})
export class SidebarComponent {
  visible = input.required<boolean>();
  close = output<void>();

  navItems: NavItem[] = [
    { label: 'Documents', icon: 'pi-file', route: '/documents' },
    { label: 'Chat', icon: 'pi-comments', route: '/chat' },
  ];
}
