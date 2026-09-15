# Dry Clean Business Logic

This document is the operational reference for the system. Any workflow or calculation change should be reflected here before it is implemented in the API, frontend, or database schema.

## Order Intake

1. Select an active client and the receiving employee.
2. Read the client's address from `Client.address` and store it as a snapshot in `DryCleanOrder.deliveryAddress` when the order is created.
3. An order contains one or more lines. Each line records garment type, service, quantity, price, color/description, and condition details when needed.
4. Services and prices are entered manually. Carpets and special cleaning do not require a separate catalog.
5. An expected delivery date and order notes are optional.
6. Print the intake receipt after the order is created.

## Order Statuses

```text
received -> processing -> ready -> delivered
received -> cancelled
processing -> needs_review -> ready
```

- `received`: The garments have been received and processing has not started.
- `processing`: The order is being handled by the production team.
- `needs_review`: The order requires review before it can be marked ready.
- `ready`: The ironer completed the order and it is ready for delivery.
- `delivered`: The order was handed to the customer or delivery recipient.
- `cancelled`: The order was cancelled and is excluded from operational calculations.

Delivery is not allowed before `ready`. Processing can start only from `received`. Payments cannot exceed the order total.

## Productivity

- Exactly one ironer is recorded when an order is completed.
- The ironer may be selected at intake or at completion.
- Productivity is calculated once when the order is completed, not piece by piece.
- `countedQuantity` is the sum of `quantity` for lines where `countsForProductivity = true`.
- Carpets and services excluded from productivity use `countsForProductivity = false`.
- `isManualEntry` indicates that a line was entered manually; it does not determine productivity eligibility by itself.
- One `DryCleanProductivity` record is created for each completed order.

## Payments and Credit

- Full, partial, and deferred payment are allowed.
- Deferred payment does not prevent delivery.
- Accepted methods are only `cash`, `wallet`, and `instapay`.
- `paidAmount` is the sum of all payments for the order.
- `remainingAmount = max(totalAmount - paidAmount, 0)`.
- A payment greater than the remaining balance is rejected.
- Payments can be recorded at intake, during processing, at delivery, or later.

## Delivery

- Select the delivery employee from `Employee`.
- A successful delivery records the employee and delivery time, then changes the order to `delivered`.
- The printed delivery address is the snapshot stored on the order, not the client's current address.

## Calculations

- `lineTotal = quantity * unitPrice`.
- `subtotal` is the sum of all line totals.
- `totalAmount = subtotal - discountAmount`; discount is currently zero by default unless a new business rule enables it.
- Financial values cannot be negative.
- Cancelled orders are excluded from client balances and operational reports.

## Permissions and Audit

- Viewing, creating, and updating orders are separate permissions.
- Starting processing, completing an order, and delivering an order are separate permissions.
- Recording payments, printing receipts, and viewing productivity are separate permissions.
- The user's permission is not sufficient by itself; the tenant's `dry_clean` capability must also be enabled.
- Status changes and payments must be traceable through the audit log.

## Database Rules

- The new schema has no catalog, purchasing, inventory, or retail-sales invoice tables.
- Client master data is stored in `Client`; the historical delivery address is stored in `DryCleanOrder`.
- Every completed order creates one productivity record and cannot create a duplicate record for the same order.
- Critical relationships use foreign keys. Deleting an order must not cascade-delete its productivity or payment history.

## Printing and Backups

- Print the intake receipt when an order is created.
- Print the ready-for-delivery sheet when processing is completed; it can be reprinted.
- Cloudinary is optional and used only for database backups, not order processing or productivity calculations.
