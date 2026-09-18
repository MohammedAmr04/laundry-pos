# Dry Clean Permissions

Permissions are enforced in the backend with `RequirePermission` and are also used by the frontend to control menus and actions. A user must have the permission and the tenant must have the `dry_clean` capability enabled.

## Order Permissions

| Key | Purpose |
|---|---|
| `dry_clean.orders.view` | View orders and details |
| `dry_clean.orders.create` | Create and receive orders |
| `dry_clean.orders.update` | Update an order before completion |
| `dry_clean.orders.process` | Assign or change the ironer |
| `dry_clean.orders.complete` | Mark the order prepared and record productivity |
| `dry_clean.orders.deliver` | Record delivery |
| `dry_clean.orders.cancel` | Reserved for a future cancellation/correction flow |

## Payments, Reports, and Printing

| Key | Purpose |
|---|---|
| `dry_clean.payments.create` | Record full or partial payments |
| `dry_clean.reports.view` | View operational reports |
| `dry_clean.productivity.view` | View ironer productivity |
| `dry_clean.print.receipt` | Print intake and ready-for-delivery receipts |
| `dry_clean.catalog.view` | View garment/service catalog |
| `dry_clean.catalog.manage` | Create, update, and deactivate catalog items and services |

## Shared Operations

| Key | Purpose |
|---|---|
| `clients.view` | Select and view clients |
| `clients.create` | Create clients |
| `clients.update` | Update client data and address |
| `employees.view` | Select receiving staff, ironers, and delivery staff |
| `employees.manage` | Manage employees |
| `audit.view` | View the audit log |
| `settings.view` | View settings |
| `settings.update` | Update settings |
| `users.manage` | Manage users |
| `roles.manage` | Manage roles and permissions |

## Default Roles

- `Admin`: all permissions.
- `Manager`: all operational and administration permissions except user and role management.
- `Cashier`: clients, employee selection, order intake, preparation, delivery, payments, printing, and catalog viewing.

## Security Rules

- The frontend must not be the only access control; the API returns `403` when permission is missing.
- Do not add retail-sales permissions to new roles.
- Every status transition records an actor in `DryCleanOrderStatusHistory`; every payment records `createdByUserId`. Catalog and backup administration actions are recorded in `AuditLog`.
