import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirmation-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (visible) {
      <div class="modal-overlay" (click)="onCancel()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2>{{ title }}</h2>
          </div>
          <p>{{ message }}</p>
          <div class="modal-actions">
            <button class="btn btn-secondary" (click)="onCancel()">Cancel</button>
            <button class="btn btn-danger" (click)="onConfirm()">{{ confirmText }}</button>
          </div>
        </div>
      </div>
    }
  `,
})
export class ConfirmationDialogComponent {
  @Input() visible = false;
  @Input() title = 'Confirm';
  @Input() message = 'Are you sure?';
  @Input() confirmText = 'Delete';
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  onConfirm(): void { this.confirm.emit(); }
  onCancel(): void { this.cancel.emit(); }
}