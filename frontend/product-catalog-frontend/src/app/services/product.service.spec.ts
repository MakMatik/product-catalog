import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { ProductService } from './product.service';
import { PaginatedResult, Product } from '../models/models';
import { environment } from '../../environments/environment';

describe('ProductService', () => {
  let service: ProductService;
  let httpMock: HttpTestingController;

  const mockProduct: Product = {
    id: 1,
    name: 'Test Laptop',
    description: 'A test laptop',
    sku: 'TEST-001',
    price: 999.99,
    quantity: 10,
    categoryId: 2,
    categoryName: 'Laptops',
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  };

  const mockPaginated: PaginatedResult<Product> = {
    items: [mockProduct],
    totalCount: 1,
    page: 1,
    pageSize: 10,
    totalPages: 1,
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        ProductService,
      ],
    });

    service = TestBed.inject(ProductService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('getProducts', () => {
    it('should fetch paginated products', () => {
      service.getProducts(1, 10).subscribe((result) => {
        expect(result.items.length).toBe(1);
        expect(result.totalCount).toBe(1);
        expect(result.items[0].name).toBe('Test Laptop');
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/products?page=1&pageSize=10`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockPaginated);
    });

    it('should include categoryId when filtering', () => {
      service.getProducts(1, 10, 2).subscribe();

      const req = httpMock.expectOne(
        `${environment.apiUrl}/products?page=1&pageSize=10&categoryId=2`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockPaginated);
    });

    it('should include search term when provided', () => {
      service.getProducts(1, 10, undefined, 'laptop').subscribe();

      const req = httpMock.expectOne(
        `${environment.apiUrl}/products?page=1&pageSize=10&search=laptop`
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockPaginated);
    });

    it('should set loading to true then false', () => {
      const states: boolean[] = [];
      service.loading$.subscribe((l) => states.push(l));

      service.getProducts(1, 10).subscribe();

      const req = httpMock.expectOne(
        `${environment.apiUrl}/products?page=1&pageSize=10`
      );
      req.flush(mockPaginated);

      expect(states).toContain(true);
      expect(states[states.length - 1]).toBe(false);
    });

    it('should handle errors and update error state', () => {
      let errorThrown = false;

      service.getProducts(1, 10).subscribe({
        error: () => { errorThrown = true; },
      });

      const req = httpMock.expectOne(
        `${environment.apiUrl}/products?page=1&pageSize=10`
      );
      req.flush(
        { message: 'Server error' },
        { status: 500, statusText: 'Internal Server Error' }
      );

      expect(errorThrown).toBe(true);
    });
  });

  describe('getProduct', () => {
    it('should fetch a single product by id', () => {
      service.getProduct(1).subscribe((product) => {
        expect(product.id).toBe(1);
        expect(product.name).toBe('Test Laptop');
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/products/1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockProduct);
    });
  });

  describe('createProduct', () => {
    it('should POST a new product', () => {
      const newProduct = {
        name: 'New Product',
        description: 'Description',
        sku: 'NEW-001',
        price: 49.99,
        quantity: 5,
        categoryId: 1,
      };

      service.createProduct(newProduct).subscribe((result) => {
        expect(result.id).toBe(1);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/products`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(newProduct);
      req.flush(mockProduct);
    });
  });

  describe('updateProduct', () => {
    it('should PUT an updated product', () => {
      const update = {
        name: 'Updated',
        description: 'Updated desc',
        sku: 'UPD-001',
        price: 59.99,
        quantity: 15,
        categoryId: 2,
      };

      service.updateProduct(1, update).subscribe((result) => {
        expect(result.id).toBe(1);
      });

      const req = httpMock.expectOne(`${environment.apiUrl}/products/1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(update);
      req.flush(mockProduct);
    });
  });

  describe('deleteProduct', () => {
    it('should DELETE a product', () => {
      service.deleteProduct(1).subscribe();

      const req = httpMock.expectOne(`${environment.apiUrl}/products/1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});