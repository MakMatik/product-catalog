import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Category, Product, CreateProductRequest, UpdateProductRequest } from '../../models/models';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    @if (visible) {
      <div class="modal-overlay" (click)="onCancel()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h2>{{ product ? 'Edit Product' : 'Add Product' }}</h2>
            <button class="btn btn-sm btn-secondary" (click)="onCancel()">&times;</button>
          </div>

          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <div class="form-group">
              <label for="name">Name *</label>
              <input id="name" formControlName="name" class="form-control" />
              @if (form.get('name')?.invalid && form.get('name')?.touched) {
                <div class="error-message">
                  @if (form.get('name')?.errors?.['required']) { Name is required. }
                  @if (form.get('name')?.errors?.['maxlength']) { Max 200 characters. }
                </div>
              }
            </div>

            <div class="form-group">
              <label for="sku">SKU *</label>
              <input id="sku" formControlName="sku" class="form-control" />
              @if (form.get('sku')?.invalid && form.get('sku')?.touched) {
                <div class="error-message">SKU is required.</div>
              }
            </div>

            <div class="form-group">
              <label for="description">Description</label>
              <textarea id="description" formControlName="description"
                class="form-control" rows="3" style="resize:vertical"></textarea>
            </div>

            <div style="display:flex;gap:12px">
              <div class="form-group" style="flex:1">
                <label for="price">Price *</label>
                <input id="price" type="number" formControlName="price"
                  class="form-control" min="0" step="0.01" />
                @if (form.get('price')?.invalid && form.get('price')?.touched) {
                  <div class="error-message">Price must be 0 or greater.</div>
                }
              </div>

              <div class="form-group" style="flex:1">
                <label for="quantity">Quantity *</label>
                <input id="quantity" type="number" formControlName="quantity"
                  class="form-control" min="0" step="1" />
                @if (form.get('quantity')?.invalid && form.get('quantity')?.touched) {
                  <div class="error-message">Quantity must be 0 or greater.</div>
                }
              </div>
            </div>

            <div class="form-group">
              <label for="categoryId">Category *</label>
              <select id="categoryId" formControlName="categoryId" class="form-control">
                <option [ngValue]="null" disabled>Select a category</option>
                @for (cat of categories; track cat.id) {
                  <option [ngValue]="cat.id">{{ cat.name }}</option>
                }
              </select>
              @if (form.get('categoryId')?.invalid && form.get('categoryId')?.touched) {
                <div class="error-message">Category is required.</div>
              }
            </div>

            <div class="modal-actions">
              <button type="button" class="btn btn-secondary" (click)="onCancel()">Cancel</button>
              <button type="submit" class="btn btn-primary" [disabled]="form.invalid || submitting">
                {{ submitting ? 'Saving...' : (product ? 'Update' : 'Create') }}
              </button>
            </div>
          </form>
        </div>
      </div>
    }
  `,
})
export class ProductFormComponent implements OnInit, OnChanges {
  @Input() visible = false;
  @Input() product: Product | null = null;
  @Output() save = new EventEmitter<CreateProductRequest | UpdateProductRequest>();
  @Output() cancel = new EventEmitter<void>();

  form!: FormGroup;
  categories: Category[] = [];
  submitting = false;

  constructor(private fb: FormBuilder, private categoryService: CategoryService) {}

  ngOnInit(): void {
    this.initForm();
    this.categoryService.getCategories().subscribe({
      next: (cats) => (this.categories = cats),
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['product'] || changes['visible']) {
      if (this.visible) {
        this.initForm();
        if (this.product) {
          this.form.patchValue({
            name: this.product.name,
            description: this.product.description,
            sku: this.product.sku,
            price: this.product.price,
            quantity: this.product.quantity,
            categoryId: this.product.categoryId,
          });
        }
      }
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      sku: ['', [Validators.required, Validators.maxLength(50)]],
      price: [0, [Validators.required, Validators.min(0)]],
      quantity: [0, [Validators.required, Validators.min(0)]],
      categoryId: [null, [Validators.required]],
    });
    this.submitting = false;
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting = true;
    this.save.emit(this.form.value);
  }

  onCancel(): void { this.cancel.emit(); }
}