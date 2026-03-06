import { Component, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../services/category.service';
import { Category } from '../../models/models';

@Component({
  selector: 'app-category-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <select
      class="form-control"
      [(ngModel)]="selectedCategoryId"
      (ngModelChange)="onCategoryChange($event)"
      style="min-width: 180px;"
    >
      <option [ngValue]="null">All Categories</option>
      @for (category of categories; track category.id) {
        <option [ngValue]="category.id">{{ category.name }}</option>
      }
    </select>
  `,
})
export class CategoryFilterComponent implements OnInit {
  @Output() categoryChange = new EventEmitter<number | undefined>();

  categories: Category[] = [];
  selectedCategoryId: number | null = null;

  constructor(private categoryService: CategoryService) {}

  ngOnInit(): void {
    this.categoryService.getCategories().subscribe({
      next: (categories) => (this.categories = categories),
    });
  }

  onCategoryChange(value: number | null): void {
    this.categoryChange.emit(value ?? undefined);
  }
}