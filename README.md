# Dry Clean Operations

A focused operations system for dry cleaning, garment cleaning, and ironing. It records customer orders, garment lines, receiving staff, ironers, delivery staff, order status, payments, delivery, and ironer productivity. The UI is Arabic-first and RTL; this documentation is maintained in English.

## Quick Start

Requirements: Windows, .NET Framework 4.8 Developer Pack, and Node.js.

```powershell
npm install
npx tsc --noEmit
npm run lint
npm test
npm run build
dotnet build backend-cs/pos-cs.csproj --configuration Release
```

For local development:

```powershell
npm run dev
dotnet run --project backend-cs/pos-cs.csproj
```

For the packaged application, run `start.bat`. The backend listens on `http://localhost:3001`, serves the API and static frontend, and applies all pending SQL migrations from `backend-cs/Database/Migrations/` on startup. The SQLite database is stored in the `data/` directory beside the executable.

Default credentials are `admin` / `1234`. Change them after the first login.

## Project Structure

- `backend-cs/`: C# .NET Framework 4.8, OWIN Web API, Dapper, and SQLite.
- `src/`: Next.js static export, React, TypeScript, and TanStack Query.
- `backend-cs/Database/Migrations/`: Ordered SQL migrations for the dry-clean schema and incremental updates.
- `messages/ar.json`: Arabic UI translations.
- `docs/DRY-CLEAN-PLAN.md`: Delivery plan.
- `docs/DRY-CLEAN-SPEC.md`: Detailed functional specification.
- [`BUSINESS-LOGIC.md`](BUSINESS-LOGIC.md): Workflow and business constraints.
- `docs/PERMISSIONS.md`: Roles and permission keys.

## Commands

| Command | Purpose |
|---|---|
| `npm run dev` | Start the frontend development server |
| `npm run build` | Build the frontend as a static export |
| `npm run lint` | Run ESLint |
| `npm test` | Run Vitest |
| `dotnet build backend-cs/pos-cs.csproj --configuration Release` | Build the backend |

## Database ERD

```mermaid
erDiagram
    CLIENT ||--o{ DRY_CLEAN_ORDER : places
    EMPLOYEE ||--o{ DRY_CLEAN_ORDER : receives
    EMPLOYEE ||--o{ DRY_CLEAN_ORDER : irons
    EMPLOYEE ||--o{ DRY_CLEAN_ORDER : delivers
    DRY_CLEAN_ORDER ||--|{ DRY_CLEAN_ORDER_LINE : contains
    DRY_CLEAN_ORDER ||--o{ DRY_CLEAN_ORDER_PAYMENT : receives
    DRY_CLEAN_ORDER ||--o{ DRY_CLEAN_ORDER_STATUS_HISTORY : tracks
    DRY_CLEAN_ORDER ||--o| DRY_CLEAN_PRODUCTIVITY : produces
    EMPLOYEE ||--o{ DRY_CLEAN_PRODUCTIVITY : earns
    USER ||--o{ DRY_CLEAN_ORDER_STATUS_HISTORY : changes
    USER ||--o{ DRY_CLEAN_ORDER_PAYMENT : records
    DRY_CLEAN_SERVICE_DEFINITION ||--o{ DRY_CLEAN_CATALOG_ITEM : defines

    CLIENT { string id PK string name string phone string address }
    EMPLOYEE { string id PK string name string phone boolean isActive }
    DRY_CLEAN_ORDER { string id PK int orderNumber UK string clientId FK string deliveryAddress string status decimal totalAmount }
    DRY_CLEAN_ORDER_LINE { string id PK string orderId FK string garmentType string serviceDescription int quantity decimal unitPrice boolean countsForProductivity boolean isManualEntry }
    DRY_CLEAN_ORDER_PAYMENT { string id PK string orderId FK decimal amount string paymentMethod datetime paidAt }
    DRY_CLEAN_PRODUCTIVITY { string id PK string orderId FK string ironerEmployeeId FK int countedQuantity datetime completedAt }
    DRY_CLEAN_ORDER_STATUS_HISTORY { string id PK string orderId FK string fromStatus string toStatus string changedByUserId FK }
    DRY_CLEAN_CATALOG_ITEM { string id PK string garmentType string serviceDescription decimal unitPrice boolean isActive }
    DRY_CLEAN_SERVICE_DEFINITION { string id PK string name boolean isActive }
    USER { string id PK string tenantId FK string username }
```

## Backup

Local backups and optional Cloudinary upload are available through `/api/backups`. Configure Cloudinary with `POS_CLOUDINARY_CLOUD_NAME`, `POS_CLOUDINARY_API_KEY`, and `POS_CLOUDINARY_API_SECRET`. The application remains usable locally when these variables are not configured.

## Authentication and Permissions

All endpoints except login and license checks require a Bearer token and the relevant permission. See `docs/PERMISSIONS.md` and `BUSINESS-LOGIC.md` for the access rules.
