# Inventory Management System

A three-tier ASP.NET Core 8 MVC application for managing stock: products,
categories, stock movements, users and roles, a real-time chat, and scheduled
expiry alerts.

---

## Requirements

| Tool | Version |
|------|---------|
| .NET SDK | 8.0 or later |
| SQL Server | Express, Developer, or LocalDB |
| Visual Studio 2022 | 17.8+ (optional — the CLI works too) |

---

## Running it

```bash
git clone https://github.com/khaled20221015/InventoryManagementSystem.git
cd InventoryManagementSystem
dotnet run --project InventoryManagementSystem.Presentation
```

Then open **https://localhost:7101**.

The database is created automatically on first run: migrations are applied,
the two roles are created, an administrator account is seeded, and a set of
demo products is inserted. No manual database setup is needed.

### Sign in

| Field | Value |
|-------|-------|
| Email | `admin@inventory.com` |
| Password | `Admin@123` |

New accounts can only be created by an administrator, from **Users → + New User**.
Every new account starts with the `Employee` role.

---

## Database connection

The connection string lives in
[`appsettings.json`](InventoryManagementSystem.Presentation/appsettings.json):

```json
"DefaultConnection": "Server=.;Database=InventoryManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
```

`Server=.` means the default SQL Server instance on this machine. If the app
cannot connect on startup, change that one line to match your setup:

| Setup | Connection string |
|-------|-------------------|
| SQL Server Express | `Server=.\\SQLEXPRESS;Database=InventoryManagementDb;Trusted_Connection=True;TrustServerCertificate=True` |
| LocalDB (ships with Visual Studio) | `Server=(localdb)\\MSSQLLocalDB;Database=InventoryManagementDb;Trusted_Connection=True;TrustServerCertificate=True` |
| SQL Server with a login | `Server=localhost;Database=InventoryManagementDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True` |

Nothing else needs changing — the app creates the database itself.

---

## Features

**Products** — CRUD, search and filter by category, server-side pagination
(page size in [`ProductService.PageSize`](InventoryManagementSystem.Business/Services/ProductService.cs)),
numbered rows, and an expiry date per product.

**Expiry tracking** — products expiring within
[`ExpiryRules.WarningDays`](InventoryManagementSystem.Business/Rules/ExpiryRules.cs)
are flagged on the dashboard. Products with two days or less of shelf life are
refused at entry, so nothing about to expire enters stock.

**Dashboard** — totals, a category breakdown and a stock chart, plus low-stock
and expiry alert tables. Each tile links to the page it summarises.

**Stock movements** — record stock in and out; the movement and the new
quantity are saved together, and stock can never go negative.

**Users and roles** — administrators create accounts, change roles, and soft
delete or restore users. The last administrator cannot be deleted.

**Chat** — real-time private messaging over SignalR, with online indicators,
unread counts, searchable contacts, stored history, and an admin broadcast to
all employees.

**Scheduled alerts** — a Hangfire job emails an administrator about products
close to expiry. The dashboard is at `/hangfire` and is restricted to
administrators.

---

## Optional: email alerts

The expiry alert job is **switched off by default**. To enable it, uncomment the
`RecurringJob.AddOrUpdate` block near the end of
[`Program.cs`](InventoryManagementSystem.Presentation/Program.cs) and remove the
`RecurringJob.RemoveIfExists` line below it.

SMTP credentials are deliberately not in the repository. Without them the app
still runs: each alert is written to `App_Data/mail/` as an HTML file instead of
being sent. To send real mail, set the password outside source control:

```bash
dotnet user-secrets set "Email:Password" "your-app-password" --project InventoryManagementSystem.Presentation
```

and set `Email:UserName` and `Email:AlertRecipient` in `appsettings.json`.

---

## Project structure

```
InventoryManagementSystem.DataAccess     entities, DbContext, migrations, repositories
InventoryManagementSystem.Business       DTOs, business rules, services
InventoryManagementSystem.Presentation   controllers, views, SignalR hub, jobs, wwwroot
```

Dependencies point one way only: Presentation depends on Business, Business
depends on DataAccess. A controller never touches a repository or `DbContext`
directly.
