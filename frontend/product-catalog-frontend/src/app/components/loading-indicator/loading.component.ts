import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loading) {
      <div style="display:flex;flex-direction:column;align-items:center;padding:40px;color:var(--gray-500)">
        <div class="spinner"></div>
        <p>{{ message }}</p>
      </div>
    }
  `,
  styles: [`
    .spinner {
      width: 36px; height: 36px;
      border: 3px solid var(--gray-200);
      border-top-color: var(--primary);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin-bottom: 12px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `],
})
export class LoadingComponent {
  @Input() loading = false;
  @Input() message = 'Loading...';
}