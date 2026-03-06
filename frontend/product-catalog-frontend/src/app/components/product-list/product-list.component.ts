import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { ProductService } from '../../services/product.service';
import { NotificationService } from '../../services/notification.service';
import { Product, CreateProductRequest, UpdateProductRequest } from '../../models/models';
import { SearchBarComponent } from '../search-bar/search-bar.component';
import { CategoryFilterComponent } from '../category-filter/category-filter.component';
import { ProductFormComponent } from '../product-form/product-form.component';
import { LoadingComponent } from '../loading-indicator/loading.component';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule, SearchBarComponent, CategoryFilterComponent,
    ProductFormComponent, LoadingComponent, ConfirmationDialogComponent,
  ],
  template: `
    <div class="page-header">
      <h1>Products</h1>
      <button class="btn btn-primary" (click)="openAddForm()">+ Add Product</button>
    </div>

    <div class="toolbar">
      <app-search-bar (search)="onSearch($event)"></app-search-bar>
      <app-category-filter (categoryChange)="onCategoryChange($event)"></app-category-filter>
    </div>

    @if (errorMessage) {
      <div style="display:flex;justify-content:space-between;align-items:center;padding:12px 16px;background:#fee2e2;color:#991b1b;border-radius:var(--radius);margin-bottom:16px;font-size:14px">
        <span>{{ errorMessage }}</span>
        <button class="btn btn-sm btn-secondary" (click)="loadProducts()">Retry</button>
      </div>
    }

    <app-loading [loading]="loading"></app-loading>

    @if (!loading && products.length > 0) {
      <div class="card">
        <table class="table">
          <thead>
            <tr>
              <th>Name</th>
              <th>SKU</th>
              <th>Category</th>
              <th>Price</th>
              <th>Stock</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (product of products; track product.id) {
              <tr>
                <td>
                  <strong>{{ product.name }}</strong><br/>
                  <small style="color:var(--gray-500)">
                    {{ product.description.length > 60
                       ? product.description.slice(0, 60) + '...'
                       : product.description }}
                  </small>
                </td>
                <td><code>{{ product.sku }}</code></td>
                <td>{{ product.categoryName || 'N/A' }}</td>
                <td>{{ product.price | currency:'USD' }}</td>
                <td>
                  <span [class]="getStockBadge(product.quantity)">
                    {{ product.quantity }}
                  </span>
                </td>
                <td>
                  <div style="display:flex;gap:6px">
                    <button class="btn btn-sm btn-secondary" (click)="openEditForm(product)">Edit</button>
                    <button class="btn btn-sm btn-danger" (click)="confirmDelete(product)">Delete</button>
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>

        @if (totalPages > 1) {
          <div class="pagination">
            <button [disabled]="currentPage === 1" (click)="goToPage(currentPage - 1)">Previous</button>
            @for (p of pages; track p) {
              <button [class.active]="p === currentPage" (click)="goToPage(p)">{{ p }}</button>
            }
            <button [disabled]="currentPage === totalPages" (click)="goToPage(currentPage + 1)">Next</button>
          </div>
        }

        <div style="text-align:center;margin-top:8px;color:var(--gray-500);font-size:13px">
          Showing {{ products.length }} of {{ totalCount }} products
        </div>
      </div>
    }

    @if (!loading && products.length === 0 && !errorMessage) {
      <div class="card" style="text-align:center;padding:40px;color:var(--gray-500)">
        <p>No products found.</p>
        <button class="btn btn-primary" style="margin-top:12px" (click)="openAddForm()">
          Add your first product
        </button>
      </div>
    }

    <app-product-form
      [visible]="showForm"
      [product]="editingProduct"
      (save)="onSave($event)"
      (cancel)="closeForm()"
    ></app-product-form>

    <app-confirmation-dialog
      [visible]="showDeleteConfirm"
      title="Delete Product"
      [message]="'Are you sure you want to delete \\'' + (deletingProduct?.name || '') + '\\'?'"
      (confirm)="onDeleteConfirm()"
      (cancel)="showDeleteConfirm = false"
    ></app-confirmation-dialog>
  `,
})
export class ProductListComponent implements OnInit, OnDestroy {
  products: Product[] = [];
  loading = false;
  errorMessage: string | null = null;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  pages: number[] = [];
  searchTerm = '';
  categoryFilter?: number;
  showForm = false;
  editingProduct: Product | null = null;
  showDeleteConfirm = false;
  deletingProduct: Product | null = null;
  private subs: Subscription[] = [];

  constructor(
    private productService: ProductService,
    private notify: NotificationService
  ) {}

  ngOnInit(): void { this.loadProducts(); }
  ngOnDestroy(): void { this.subs.forEach((s) => s.unsubscribe()); }

  loadProducts(): void {
    this.loading = true;
    this.errorMessage = null;
    this.subs.push(
      this.productService
        .getProducts(this.currentPage, this.pageSize, this.categoryFilter, this.searchTerm)
        .subscribe({
          next: (result) => {
            this.products = result.items;
            this.totalCount = result.totalCount;
            this.totalPages = result.totalPages;
            this.pages = this.generatePages();
            this.loading = false;
          },
          error: (err) => { this.errorMessage = err.message; this.loading = false; },
        })
    );
  }

  onSearch(term: string): void { this.searchTerm = term; this.currentPage = 1; this.loadProducts(); }
  onCategoryChange(id: number | undefined): void { this.categoryFilter = id; this.currentPage = 1; this.loadProducts(); }
  goToPage(page: number): void { if (page >= 1 && page <= this.totalPages) { this.currentPage = page; this.loadProducts(); } }

  openAddForm(): void { this.editingProduct = null; this.showForm = true; }
  openEditForm(p: Product): void { this.editingProduct = { ...p }; this.showForm = true; }
  closeForm(): void { this.showForm = false; this.editingProduct = null; }

  onSave(data: CreateProductRequest | UpdateProductRequest): void {
    if (this.editingProduct) {
      this.subs.push(
        this.productService.updateProduct(this.editingProduct.id, data as UpdateProductRequest).subscribe({
          next: () => { this.notify.success('Product updated'); this.closeForm(); this.loadProducts(); },
          error: (err) => this.notify.error(err.message),
        })
      );
    } else {
      this.subs.push(
        this.productService.createProduct(data as CreateProductRequest).subscribe({
          next: () => { this.notify.success('Product created'); this.closeForm(); this.loadProducts(); },
          error: (err) => this.notify.error(err.message),
        })
      );
    }
  }

  confirmDelete(p: Product): void { this.deletingProduct = p; this.showDeleteConfirm = true; }

  onDeleteConfirm(): void {
    if (!this.deletingProduct) return;
    this.subs.push(
      this.productService.deleteProduct(this.deletingProduct.id).subscribe({
        next: () => { this.notify.success('Product deleted'); this.showDeleteConfirm = false; this.loadProducts(); },
        error: (err) => { this.notify.error(err.message); this.showDeleteConfirm = false; },
      })
    );
  }

  getStockBadge(qty: number): string {
    if (qty === 0) return 'badge badge-danger';
    if (qty <= 10) return 'badge badge-warning';
    return 'badge badge-success';
  }

  private generatePages(): number[] {
    const pages: number[] = [];
    const max = 5;
    let start = Math.max(1, this.currentPage - Math.floor(max / 2));
    let end = Math.min(this.totalPages, start + max - 1);
    if (end - start + 1 < max) start = Math.max(1, end - max + 1);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }
}