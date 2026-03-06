import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, catchError, throwError, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Category, CategoryTree, CreateCategoryRequest } from '../models/models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly apiUrl = `${environment.apiUrl}/categories`;

  private categoriesSubject = new BehaviorSubject<Category[]>([]);
  private categoryTreeSubject = new BehaviorSubject<CategoryTree[]>([]);

  categories$ = this.categoriesSubject.asObservable();
  categoryTree$ = this.categoryTreeSubject.asObservable();

  constructor(private http: HttpClient) {}

  getCategories(): Observable<Category[]> {
    return this.http.get<Category[]>(this.apiUrl).pipe(
      tap((categories) => this.categoriesSubject.next(categories)),
      catchError((error) => {
        return throwError(() => new Error(error.error?.message || 'Failed to load categories'));
      })
    );
  }

  getCategoryTree(): Observable<CategoryTree[]> {
    return this.http.get<CategoryTree[]>(`${this.apiUrl}/tree`).pipe(
      tap((tree) => this.categoryTreeSubject.next(tree)),
      catchError((error) => {
        return throwError(() => new Error(error.error?.message || 'Failed to load category tree'));
      })
    );
  }

  createCategory(category: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, category).pipe(
      catchError((error) => {
        return throwError(() => new Error(error.error?.message || 'Failed to create category'));
      })
    );
  }
}