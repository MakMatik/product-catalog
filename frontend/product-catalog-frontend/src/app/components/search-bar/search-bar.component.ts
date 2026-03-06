import { Component, Output, EventEmitter, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, Subscription } from 'rxjs';

@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <input
      type="text"
      class="form-control"
      placeholder="Search products by name, SKU, or description..."
      [(ngModel)]="searchTerm"
      (ngModelChange)="onSearchChange($event)"
      style="min-width: 300px;"
    />
  `,
})
export class SearchBarComponent implements OnInit, OnDestroy {
  @Output() search = new EventEmitter<string>();

  searchTerm = '';
  private searchSubject = new Subject<string>();
  private subscription?: Subscription;

  ngOnInit(): void {
    this.subscription = this.searchSubject
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe((term) => this.search.emit(term));
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  onSearchChange(value: string): void {
    this.searchSubject.next(value);
  }
}