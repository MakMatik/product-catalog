export interface Product {
  id: number;
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
  categoryName: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
}

export interface UpdateProductRequest {
  name: string;
  description: string;
  sku: string;
  price: number;
  quantity: number;
  categoryId: number;
}

export interface Category {
  id: number;
  name: string;
  description: string;
  parentCategoryId: number | null;
}

export interface CategoryTree {
  id: number;
  name: string;
  description: string;
  parentCategoryId: number | null;
  children: CategoryTree[];
}

export interface CreateCategoryRequest {
  name: string;
  description: string;
  parentCategoryId: number | null;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  statusCode: number;
}