# Inventory Management System

A small ASP.NET Core 8 MVC application for managing products, categories and stock movements,
built with a **Three-Tier Architecture**.

## Projects

| Project | Layer | What lives here |
| --- | --- | --- |
| `InventoryManagementSystem.Presentation` | Presentation | Controllers, Razor views, the REST API, `Program.cs` |
| `InventoryManagementSystem.Business` | Business Logic | Services (the rules), DTOs (validation) |
| `InventoryManagementSystem.DataAccess` | Data Access | EF Core models, `ApplicationDbContext`, repositories, Identity, migrations |

Dependencies only point downwards: **Presentation → Business → DataAccess**.
The Presentation layer never touches the database directly.

## How a request flows

Taking "create a product" as the example:

```
Products/Create.cshtml   →  ProductsController.Create(ProductFormDto)   (Presentation)
                         →  ProductService.CreateAsync(dto)             (Business: rules)
                         →  ProductRepository.AddAsync(product)         (DataAccess: EF Core)
                         →  SQL Server
```

Validation happens in two places:

- **DTO attributes** (`ProductFormDto`) catch bad input — required fields, lengths, ranges.
- **Services** catch business rules — duplicate names, unknown category, not enough stock.
  A service returns `null` when everything is fine, or an error message string when it is not.

## Roles

| Role | Can do |
| --- | --- |
| `Admin` | Everything: products CRUD, categories, users, dashboard, stock |
| `Employee` | View and search products, view details, record stock movements, dashboard |

Role names live in `RoleNames.cs`. New accounts created through **Register** get `Employee`.

Seeded on first run by `DbSeeder`:

- Email: `admin@inventory.com`
- Password: `Admin@123`

## Features

- Registration, login and logout (ASP.NET Core Identity)
- Full CRUD for products and categories
- Product search and filter by category
- Stock movements (In / Out) with an audit trail of who recorded each one
- Low-stock alerts on the dashboard
- Dashboard charts (Chart.js) — products per category, stock per product
- REST API for products at `/api/products`, documented with Swagger in development
- One friendly error page for 404, 403 and unhandled errors

## Database

One migration, `InitialCreate`, builds the whole schema shown in the design document:
`Categories`, `Products`, `StockTransactions` and the ASP.NET Identity tables.

You do **not** run any database command by hand. On every start `DbSeeder` calls
`Database.Migrate()`, which creates the database if it is missing and applies the
migration. That is what makes the app work on a fresh machine such as the IIS server.

The connection string is in `InventoryManagementSystem.Presentation/appsettings.json`
and points at the local SQL Server instance (`Server=.`).

## Running it

```bash
dotnet run --project InventoryManagementSystem.Presentation
```

Swagger is available at `/swagger` while running in Development.
All front-end libraries (Bootstrap, jQuery, Chart.js) are served from `wwwroot/lib`,
so the app needs no internet connection.
