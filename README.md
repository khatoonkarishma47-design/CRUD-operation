# Product Service Microservice

A .NET Core Web API microservice with CRUD operations, Entity Framework, JWT authentication, and Swagger documentation.

## Features

- **CRUD Operations**: Full Create, Read, Update, Delete operations for Products
- **Entity Framework**: Uses EF Core with InMemory database (configurable for SQL Server)
- **JWT Authentication**: Secure API endpoints with JWT token validation
- **Swagger Documentation**: Interactive API documentation at `/swagger`
- **Default Data**: Returns seed data when database is not available
- **Unit Tests**: Comprehensive test coverage for services

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Node.js (for React UI)

### Running the API

```bash
cd ProductService
dotnet run
```

The API will be available at:
- HTTPS: https://localhost:7001
- HTTP: http://localhost:5000
- Swagger UI: https://localhost:7001/swagger

### Running Tests

```bash
cd ProductService.Tests
dotnet test
```

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login and get JWT token |
| POST | `/api/auth/register` | Register a new user |

### Products (Requires Authentication)

| Method | Endpoint | Description | Role |
|--------|----------|-------------|------|
| GET | `/api/products` | Get all products | User, Admin |
| GET | `/api/products/{id}` | Get product by ID | User, Admin |
| POST | `/api/products` | Create a product | Admin |
| PUT | `/api/products/{id}` | Update a product | Admin |
| DELETE | `/api/products/{id}` | Delete a product | Admin |

## Default Credentials

| Username | Password | Role |
|----------|----------|------|
| admin | admin123 | Admin |
| user | user123 | User |

## Configuration

Edit `appsettings.json` to configure:

```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "ProductService",
    "Audience": "ProductServiceClient"
  },
  "UseInMemoryDatabase": true,
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProductServiceDb;..."
  }
}
```

Set `UseInMemoryDatabase` to `false` to use SQL Server.

## Project Structure

```
ProductService/
├── Controllers/
│   ├── AuthController.cs
│   └── ProductsController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Models/
│   ├── Product.cs
│   ├── User.cs
│   └── LoginRequest.cs
├── Services/
│   ├── IProductService.cs
│   ├── ProductService.cs
│   ├── IAuthService.cs
│   └── AuthService.cs
├── Program.cs
└── appsettings.json
```

## React UI

The React UI is located in the `product-ui` folder.

### Running the UI

```bash
cd product-ui
npm install
npm start
```

The UI will be available at http://localhost:3000
