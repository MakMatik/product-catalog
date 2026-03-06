import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { NotificationService, Notification } from './services/notification.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <nav class="navbar">
      <div class="container">
        <a class="navbar-brand" routerLink="/">Product Catalog</a>
        <ul class="navbar-nav">
          <li>
            <a routerLink="/products" routerLinkActive="active">Products</a>
          </li>
          <li>
            <a routerLink="/categories" routerLinkActive="active">Categories</a>
          </li>
        </ul>
      </div>
    </nav>

    <main class="container">
      <router-outlet></router-outlet>
    </main>

    <div class="toast-container">
      @for (notification of notifications; track notification.id) {
        <div
          class="toast toast-{{ notification.type }}"
          (click)="dismiss(notification.id)"
        >
          {{ notification.message }}
        </div>
      }
    </div>
  `,
})
export class AppComponent {
  notifications: Notification[] = [];

  constructor(private notificationService: NotificationService) {
    this.notificationService.notifications$.subscribe(
      (n) => (this.notifications = n)
    );
  }

  dismiss(id: number): void {
    this.notificationService.dismiss(id);
  }
}