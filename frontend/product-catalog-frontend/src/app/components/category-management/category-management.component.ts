import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoryService } from '../../services/category.service';
import { NotificationService } from '../../services/notification.service';
import { Category, CategoryTree, CreateCategoryRequest } from '../../models/models';
import { LoadingComponent } from '../loading-indicator/loading.component';

@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LoadingComponent],
  template: `
    <div class="page-header">
      <h1>Categories</h1>
      <button class="btn btn-primary" (click)="showForm = !showForm">
        {{ showForm ? 'Cancel' : '+ Add Category' }}
      </button>
    </div>

    @if (showForm) {
      <div class="card" style="margin-bottom:20px">
        <h3 style="margin-bottom:16px">New Category</h3>
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="catName">Name *</label>
            <input id="catName" formControlName="name" class="form-control" />
            @if (form.get('name')?.invalid && form.get('name')?.touched) {
              <div class="error-message">Name is required (max 100 chars).</div>
            }
          </div>
          <div class="form-group">
            <label for="catDesc">Description</label>
            <input id="catDesc" formControlName="description" class="form-control" />
          </div>
          <div class="form-group">
            <label for="parentId">Parent Category</label>
            <select id="parentId" formControlName="parentCategoryId" class="form-control">
              <option [ngValue]="null">None (Root Category)</option>
              @for (cat of flatCategories; track cat.id) {
                <option [ngValue]="cat.id">{{ cat.name }}</option>
              }
            </select>
          </div>
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid">Create</button>
        </form>
      </div>
    }

    <app-loading [loading]="loading"></app-loading>

    @if (!loading && categoryTree.length > 0) {
      <div class="card">
        <h3 style="margin-bottom:16px">Category Hierarchy</h3>
        @for (root of categoryTree; track root.id) {
          <div style="margin-bottom:4px">
            <div style="padding:6px 10px;border-left:3px solid var(--primary);font-size:14px">
              <strong>{{ root.name }}</strong>
              <span style="color:var(--gray-500);margin-left:8px">{{ root.description }}</span>
            </div>
            @for (child of root.children; track child.id) {
              <div style="margin-left:24px;padding:6px 10px;border-left:2px solid var(--gray-200);font-size:14px">
                {{ child.name }}
                <span style="color:var(--gray-500);margin-left:8px">{{ child.description }}</span>
              </div>
            }
          </div>
        }
      </div>
    }

    @if (!loading && flatCategories.length > 0) {
      <div class="card" style="margin-top:20px">
        <h3 style="margin-bottom:16px">All Categories</h3>
        <table class="table">
          <thead>
            <tr><th>ID</th><th>Name</th><th>Description</th><th>Parent</th></tr>
          </thead>
          <tbody>
            @for (cat of flatCategories; track cat.id) {
              <tr>
                <td>{{ cat.id }}</td>
                <td>{{ cat.name }}</td>
                <td>{{ cat.description }}</td>
                <td>{{ getParentName(cat.parentCategoryId) }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>
    }
  `,
})
export class CategoryManagementComponent implements OnInit {
  categoryTree: CategoryTree[] = [];
  flatCategories: Category[] = [];
  loading = false;
  showForm = false;
  form!: FormGroup;

  constructor(
    private categoryService: CategoryService,
    private notify: NotificationService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      parentCategoryId: [null],
    });
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading = true;
    this.categoryService.getCategories().subscribe({ next: (c) => (this.flatCategories = c) });
    this.categoryService.getCategoryTree().subscribe({
      next: (t) => { this.categoryTree = t; this.loading = false; },
      error: () => (this.loading = false),
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.categoryService.createCategory(this.form.value as CreateCategoryRequest).subscribe({
      next: () => { this.notify.success('Category created'); this.showForm = false; this.form.reset(); this.loadCategories(); },
      error: (err) => this.notify.error(err.message),
    });
  }

  getParentName(id: number | null): string {
    if (!id) return '—';
    return this.flatCategories.find((c) => c.id === id)?.name || 'Unknown';
  }
}