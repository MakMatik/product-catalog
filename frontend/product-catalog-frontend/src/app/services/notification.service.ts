import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Notification {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning';
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private notifications: Notification[] = [];
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  private nextId = 0;

  notifications$ = this.notificationsSubject.asObservable();

  success(message: string): void { this.add(message, 'success'); }
  error(message: string): void { this.add(message, 'error'); }
  warning(message: string): void { this.add(message, 'warning'); }

  dismiss(id: number): void {
    this.notifications = this.notifications.filter((n) => n.id !== id);
    this.notificationsSubject.next([...this.notifications]);
  }

  private add(message: string, type: 'success' | 'error' | 'warning'): void {
    const notification: Notification = { id: this.nextId++, message, type };
    this.notifications.push(notification);
    this.notificationsSubject.next([...this.notifications]);
    setTimeout(() => this.dismiss(notification.id), 4000);
  }
}