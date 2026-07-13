# 🛒 E-Commerce API

A backend API for an online store built with **ASP.NET Core Web API**, **Entity Framework Core**, and **Redis**, structured using **Onion Architecture** with strict dependency inversion around a persistence-agnostic core.

> ⚠️ **Status:** Active development. Product catalog, basket, and authentication are functional; order processing and payment integration are on the roadmap (see below).

---

## 📐 Architecture

The solution follows **Onion Architecture**: concentric layers around a dependency-free core, where every outer ring depends inward and never the reverse. Dependencies are inverted at the boundary — `Infrastructure` *implements* contracts that `Domain` defines, rather than `Domain` depending on `Infrastructure`.

```
                ┌─────────────────────────────┐
                │   E_Commerce.API              │  ← Outer ring: composition root,
                │   (Controllers, Filters,       │     references Application AND
                │    Program.cs)                 │     Infrastructure directly
                └───────────┬─────────────────┘
                            │
        ┌───────────────────┴───────────────────┐
        │       E_Commerce.Infrastructure          │  ← Outer ring: implements the
        │  (EF Core, Redis, Identity, JWT)          │     contracts defined in Domain
        └───────────────────┬───────────────────┘
                            │ implements
        ┌───────────────────┴───────────────────┐
        │       E_Commerce.Application             │  ← Use cases / business rules,
        │  (Services, DTOs, Specifications)         │     depends only on Domain
        └───────────────────┬───────────────────┘
                            │
                ┌───────────┴─────────────────┐
                │       E_Commerce.Domain        │  ← Core: entities + contracts,
                │  (Entities, Contracts)         │     zero external dependencies
                └─────────────────────────────┘
```

| Layer | Project | Responsibility |
|---|---|---|
| **Domain** *(core)* | `E_Commerce.Domain` | Core entities, base types, and repository/specification **contracts** (interfaces only — no implementation, no external dependencies) |
| **Application** | `E_Commerce.Application` | Services (use cases), DTOs, AutoMapper profiles, Specifications, the cross-cutting `Result`/`Error` types — depends only on `Domain` |
| **Infrastructure** | `E_Commerce.Infrastructure` | EF Core `DbContext` (SQL Server), Identity, Redis-backed repositories, generic repository + Unit of Work — *implements* `Domain`'s contracts |
| **API** *(outermost)* | `E_Commerce.API` | Controllers, action filters, DI composition root (`Program.cs`), Swagger — the only project referencing both `Application` and `Infrastructure` |

Because the repository/specification **contracts live in `Domain`** (`IGenericRepository`, `ISpecifications`, `IBasketRepository`, `IUnitOfWork`) and are only **implemented** in `Infrastructure`, the `Application` layer's services depend on abstractions it fully controls — never on EF Core or Redis directly. This is the defining trait of Onion Architecture (vs. a simple layered/N-Tier chain): the core has no knowledge that a database or cache even exists, and the persistence technology is fully swappable without touching business logic. Wiring only happens at the composition root, in `Program.cs`.

```
E_Commerce/
├── E_Commerce.Domain/                # Core layer — no external dependencies
│   ├── Entities/
│   │   ├── Products/                  # Product, ProductBrand, ProductType
│   │   └── Baskets/                   # CustomerBasket, BasketItem
│   ├── Common/                        # BaseEntity<TKey>
│   └── Contracts/                     # IGenericRepository, IUnitOfWork, ISpecifications,
│                                       # IBasketRepository, ICacheRepository, IDataSeeder
│
├── E_Commerce.Application/           # Use-case / business logic layer
│   ├── Services/                      # ProductService, BasketService, AuthenticationService, CacheService
│   ├── Contracts/                     # Service interfaces (IProductService, IBasketService, ITokenService...)
│   ├── DTOs/                          # Products, Baskets, Identity
│   ├── Specifications/                # BaseSpecification<T>, ProductWithTypeAndBrandSpec, ProductCountSpecification
│   ├── Common/                        # Result<T>, Error, ErrorType, PaginatedResult, ProductQueryParams
│   └── Profiles/                      # AutoMapper mapping profiles
│
├── E_Commerce.Infrastructure/        # Persistence & external services
│   ├── Data/                          # StoreDbContext, Configurations, Migrations
│   ├── Identity/                      # StoreIdentityDbContext, ApplicationUser, Address
│   ├── Repositories/                  # GenericRepository, UnitOfWork, BasketRepository (Redis), CacheRepository
│   ├── Services/                      # IdentityService, TokenService (JWT)
│   └── DataSeeding/                   # Domain + Identity seeders
│
└── E_Commerce.API/                   # Presentation layer
    ├── Controllers/                   # ProductsController, BasketsController, AuthenticationController
    ├── Attributes/                    # RedisCacheAttribute (custom output-caching action filter)
    ├── Extensions/                    # WebApplicationExtensions (migration/seeding on startup)
    └── Program.cs                     # DI composition, middleware pipeline
```

---

## 🧩 Design Patterns & Practices

- **Onion Architecture** — `Domain` sits at the core and defines contracts that `Infrastructure` implements at the outer ring, so `Application` and `Domain` never reference EF Core, Redis, or ASP.NET Identity directly; all wiring happens at the composition root in `Program.cs`.
- **Generic Repository Pattern** — `IGenericRepository<TEntity, TKey>` provides reusable CRUD + specification-based querying for any entity deriving from `BaseEntity<TKey>`, keyed generically to support both `int` (`Product`) and `string` (`CustomerBasket`) primary keys.
- **Unit of Work Pattern** — `IUnitOfWork.GetRepository<TEntity, TKey>()` lazily instantiates and caches one generic repository per entity type per request, with a single `SaveChangesAsync` centralizing commits.
- **Specification Pattern** — `BaseSpecification<TEntity, TKey>` encapsulates filtering (`Criteria`), eager-loading (`IncludeExpressions`), sorting, and pagination as composable objects (e.g. `ProductWithTypeAndBrandSpec`), evaluated centrally by a `SpecificationEvaluator` — keeping complex EF Core querying logic out of services.
- **Result Pattern** — Services return `Result` / `Result<T>` instead of throwing exceptions, carrying a typed `Error` (`ErrorType`: `NotFound`, `Validation`, `Conflict`, `Unauthorized`, `Forbidden`). `ApiBaseController.ToActionResult()` maps this uniformly to the correct HTTP status code and a standard `ProblemDetails` response.
- **Cross-Cutting Caching via Action Filter** — `RedisCacheAttribute` is a custom `ActionFilterAttribute` that short-circuits `GET` requests with a cached Redis response (keyed by path + sorted query string) and transparently caches successful `200 OK` responses with a configurable TTL — applied declaratively (`[RedisCache(90)]`) with no caching code inside controllers or services.
- **Dual-Persistence Repositories** — Relational data (`Product`, `ProductBrand`, `ProductType`) is served through the EF Core-backed `GenericRepository`, while the ephemeral `CustomerBasket` is served through a separate Redis-backed `BasketRepository` implementing the same contract shape — each store used where it fits best.
- **DTO + AutoMapper Boundary** — Controllers and services exchange `DTOs` (`ProductDto`, `BasketDto`, `UserDto`, etc.), never raw entities, mapped centrally via AutoMapper profiles.
- **Dependency Injection via Layer-Specific Registration Extensions** — Each layer exposes its own `IServiceCollection` extension (`AddInfrastructureServices`, `AddApplicationServices`), keeping `Program.cs` a thin composition root.
- **JWT-Based Authentication** — `TokenService` issues signed JWTs on login/register, validated via `Microsoft.AspNetCore.Authentication.JwtBearer`; endpoints are locked down with `[Authorize]` where needed.
- **Database & Identity Seeding on Startup** — `WebApplicationExtensions.SeedAndMigrateDataAsync()` applies pending EF Core migrations and seeds catalog + Identity data automatically at boot.

---

## 🗂️ Domain Model (current)

| Entity | Store | Notes |
|---|---|---|
| `Product` | SQL Server | Name, description, price, image URL; linked to a `ProductBrand` and `ProductType` |
| `ProductBrand` / `ProductType` | SQL Server | Lookup entities used for filtering |
| `CustomerBasket` | Redis | Client-generated `Id`, holds a collection of `BasketItem`s |
| `BasketItem` | Redis (embedded) | Snapshot of product name, image, price, and quantity at time of add |
| `ApplicationUser` | SQL Server (Identity) | Extends `IdentityUser`; linked to an `Address` |

---

## 🛠️ Tech Stack

- **.NET 8** / **ASP.NET Core Web API**
- **Entity Framework Core 8** (SQL Server provider)
- **Redis** (`StackExchange.Redis`) — basket storage + response caching
- **ASP.NET Core Identity** + **JWT Bearer Authentication**
- **AutoMapper**
- **Swagger / Swashbuckle** for API documentation

---

## 📡 Current Endpoints

**Products**
```
GET /api/Products            # Paginated products (filter by brand/type, sort, search) — Redis cached
GET /api/Products/{id}       # Get product by ID (requires auth)
GET /api/Products/brands     # Get all brands
GET /api/Products/types      # Get all types
```

**Baskets**
```
GET    /api/Baskets/{id}     # Get basket
POST   /api/Baskets          # Create/update basket
DELETE /api/Baskets/{id}     # Delete basket
```

**Authentication**
```
POST /api/Authentication/Login       # User login (returns JWT)
POST /api/Authentication/Register    # User registration
```

---

## 🚧 Roadmap

- Order processing (basket → order conversion, price re-validation, status tracking, delivery methods)
- Stripe payment integration
- Extended account endpoints (`CurrentUser`, `Address` get/update, email-exists check)

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or full instance)
- Redis instance (local or hosted)

### Setup
```bash
git clone https://github.com/<your-username>/E_Commerce.git
cd E_Commerce
```

1. Configure connection strings and JWT settings in `E_Commerce.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=E_CommerceDb;Trusted_Connection=True;",
     "Redis": "localhost:6379"
   }
   ```
2. Run the app — migrations and seed data are applied automatically on startup:
   ```bash
   dotnet run --project E_Commerce.API
   ```
3. Browse the API via Swagger at `/swagger` in development.
