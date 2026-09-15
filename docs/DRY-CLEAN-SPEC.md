# Dry Clean Edition Specification

## Objective

Provide a focused Dry Clean-only application. The system receives customer garments, tracks preparation and ironing, records the single ironer who completed an order, prints intake and ready-for-delivery receipts, manages delivery, and records full, partial, or deferred payments.

## Confirmed Business Rules

- A dry-clean order contains a customer, delivery address, garment/service lines, quantities, unit prices, and totals.
- The employee who receives the order, the ironer, and the delivery employee are recorded separately.
- One order has at most one ironer. The ironer may be assigned at creation or when the order is completed.
- Each line has a productivity flag. Only flagged quantities count toward the ironer's daily productivity.
- Productivity is credited when the order is completed, not piece-by-piece.
- Carpet and special manual-cleaning lines may be excluded from productivity.
- Service type, garment type, and price are entered manually. Historical order prices are immutable after completion.
- An intake receipt is printable when the order is created.
- The order becomes ready for delivery after ironing is completed and the ready receipt can be printed.
- Delivery may be made with a zero, partial, or outstanding balance.
- Payment methods are cash, wallet, and Instapay.
- Payments can be recorded during delivery or later.

## Order Statuses

`received -> processing -> ready -> delivered`

`received -> cancelled`

`processing -> needs_review -> ready`

Every transition records actor and timestamp. Delivered and cancelled orders are immutable except through an explicit correction flow added later.

## Core Schema

### DryCleanOrder

- `id`
- `orderNumber` (unique human-readable number)
- `clientId`
- `deliveryAddress`
- `status`
- `receivedByUserId`
- `assignedIronerId` (nullable)
- `deliveredByEmployeeId` (nullable)
- `receivedAt`
- `processingStartedAt`
- `completedAt`
- `readyAt`
- `deliveredAt`
- `cancelledAt`
- `cancelledReason`
- `subtotal`
- `discountAmount`
- `totalAmount`
- `notes`
- `createdAt`
- `updatedAt`

### DryCleanOrderLine

- `id`
- `orderId`
- `garmentType`
- `serviceDescription`
- `quantity`
- `colorOrDescription`
- `conditionBefore`
- `damageNotes`
- `processingNotes`
- `unitPrice`
- `lineTotal`
- `countsForProductivity`
- `isManualEntry`

### DryCleanOrderStatusHistory

- `id`
- `orderId`
- `fromStatus`
- `toStatus`
- `changedByUserId`
- `notes`
- `createdAt`

### DryCleanOrderPayment

- `id`
- `orderId`
- `amount`
- `paymentMethod` (`cash`, `wallet`, `instapay`)
- `paidAt`
- `createdByUserId`
- `reference`
- `notes`

### DryCleanProductivity

- `id`
- `orderId`
- `ironerEmployeeId`
- `countedQuantity`
- `completedAt`
- `createdAt`

Productivity is generated once per order completion and must be idempotent.

## Required Modules

- Dry Clean orders, lines, status history, payments, productivity, and reports
- Customers
- Employees
- Authentication, users, roles, permissions
- Printing and printer settings
- Shifts and cash reconciliation
- Expenses
- Audit log

## Removed Modules

Product catalogs, inventory, purchasing, supplier management, stock movements, bundles, wholesale pricing, and retail checkout.

## Permissions

- `dry_clean.orders.view`
- `dry_clean.orders.create`
- `dry_clean.orders.update`
- `dry_clean.orders.process`
- `dry_clean.orders.complete`
- `dry_clean.orders.deliver`
- `dry_clean.orders.cancel`
- `dry_clean.payments.create`
- `dry_clean.reports.view`
- `dry_clean.productivity.view`
- `dry_clean.print.receipt`

## Verification

- Unit tests cover status transitions, payment totals, duplicate payment/delivery prevention, and productivity idempotency.
- API tests cover authorization and server-side validation.
- Frontend typecheck, lint, tests, and static build pass.
- A fresh database applies only the new Dry Clean schema and shared operational/auth migrations.

## Confirmed Design Decisions

- Customer address is stored on the order as a snapshot, shown on the customer receipt/invoice, so later customer edits do not change delivery history.
- Receiving, ironing, and delivery workers are all selected from the `Employee` table; authenticated users remain audit actors.
- Discounts are retained as an order-level amount but no discount workflow has been specified yet.
- A delivered order cannot be edited or deleted in the first release.
