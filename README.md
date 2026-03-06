# Product Catalog Management System

Full-stack product catalog built with **ASP.NET Core 10 Web API** and **Angular 21**.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 24+](https://nodejs.org/) and npm
- [Angular CLI 21+](https://angular.dev/): `npm install -g @angular/cli`

## Running the Application

### Backend
```bash
cd backend/ProductCatalog.Api
dotnet restore
dotnet run
```

API available at **http://localhost:5050**

### Frontend
```bash
cd frontend
npm install
ng serve
```

App available at **http://localhost:4200**.

### Running Tests
```bash
cd frontend
ng test --watch=false --browsers=ChromeHeadless
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/products` | Paginated list (`page`, `pageSize`, `categoryId`, `search`, `sortBy`) |
| GET | `/api/products/{id}` | Single product |
| POST | `/api/products` | Create product |
| PUT | `/api/products/{id}` | Update product |
| DELETE | `/api/products/{id}` | Delete product |
| GET | `/api/products/search?q=` | Fuzzy search with weighted scoring |
| POST | `/api/products/manual-bind` | Manual model binding demo |
| GET | `/api/categories` | Flat category list |
| GET | `/api/categories/tree` | Hierarchical tree |
| POST | `/api/categories` | Create category |

## Project Structure
```
├── backend/ProductCatalog.Api/
│   ├── Controllers/          # API controllers
│   ├── Data/                 # EF Core DbContext
│   ├── DTOs/                 # Record-type DTOs (C# 9+)
│   ├── Extensions/           # Custom LINQ extensions
│   ├── Middleware/            # Custom middleware (from scratch)
│   ├── Models/               # Domain models with IComparable
│   ├── Repositories/         # Generic Repository<T> pattern
│   ├── Serialization/        # Custom JSON serializer
│   └── Services/             # ProductSearchEngine
└── frontend/src/app/
    ├── components/           # Standalone Angular components
    ├── models/               # TypeScript interfaces
    └── services/             # Services with RxJS state management
```