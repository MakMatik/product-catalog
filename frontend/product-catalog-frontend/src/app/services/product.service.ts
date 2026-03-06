import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, catchError, throwError, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Product,
  CreateProductRequest,
  UpdateProductRequest,
  PaginatedResult,
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly apiUrl = `${environment.apiUrl}/products`;

  private productsSubject = new BehaviorSubject<PaginatedResult<Product> | null>(null);
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string | null>(null);

  products$ = this.productsSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();
  error$ = this.errorSubject.asObservable();

  constructor(private http: HttpClient) {}

  getProducts(
    page = 1,
    pageSize = 10,
    categoryId?: number,
    search?: string,
    sortBy?: string
  ): Observable<PaginatedResult<Product>> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (categoryId) params = params.set('categoryId', categoryId.toString());
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);

    return this.http.get<PaginatedResult<Product>>(this.apiUrl, { params }).pipe(
      tap((result) => {
        this.productsSubject.next(result);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        const message = error.error?.message || 'Failed to load products';
        this.errorSubject.next(message);
        return throwError(() => new Error(message));
      })
    );
  }

  getProduct(id: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrl}/${id}`).pipe(
      catchError((error) => {
        return throwError(() => new Error(error.error?.message || 'Failed to load product'));
      })
    );
  }

  createProduct(product: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.apiUrl, product).pipe(
      catchError((error) => {
        return throwError(() => new Error(error.error?.message || 'Failed to create product'));
      })
    );
  }

  updateProduct(id: number, product: UpdateProductRequest): Observable<Product> {
    return this.http.put<Product>(`${this.apiUrl}/${id}`, product).pipe(
      catchError((error) => {
        return throwError(() => new Error(error.error?.message || 'Failed to update product'));
      })
    );
  }

  deleteProduct(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      catchError((error) => {
        return throwError(() => new Error(error.error?.message || 'Failed to delete product'));
      })
    );
  }
}