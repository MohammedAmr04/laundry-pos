# Implementation Plan: Dry Clean Only Edition

## Overview

Replace the general POS with a focused Dry Clean workflow and a clean schema. Keep only shared authentication, customer, employee, printing, shifts, expenses, audit, and operational infrastructure.

## Architecture Decisions

- Use `DryCleanOrder` instead of `Invoice`; sales invoices are coupled to inventory, FIFO, and stock side effects.
- Use dedicated dry-clean payments instead of overloading the existing payment target model.
- Use a status history table for workflow queries and a productivity fact table for idempotent daily counts.
- Build a fresh schema baseline for the Dry Clean edition; do not retain or reinterpret old sales migrations.
- Preserve authenticated users as audit actors and employees as operational workers.

## Task List

### Phase 1: Foundation

- [ ] Confirm the Dry Clean specification and remaining assumptions.
- [ ] Create a clean migration baseline containing only shared/auth and Dry Clean tables.
- [ ] Remove old POS entities, controllers, services, repositories, routes, translations, and navigation entries.
- [ ] Define domain models, requests, permissions, and API contracts.

### Checkpoint: Foundation

- [ ] Fresh database starts successfully.
- [ ] Backend compiles and schema tests pass.

### Phase 2: Core Order Flow

- [ ] Create orders and print intake receipts.
- [ ] List/search orders with server-side filters.
- [ ] Assign or record the ironer and complete an order.
- [ ] Generate productivity facts exactly once and expose daily totals.
- [ ] Transition orders to ready and print ready receipts.

### Checkpoint: Core Flow

- [ ] A multi-line order can be received, completed, counted, and marked ready.

### Phase 3: Delivery and Money

- [ ] Record full, partial, and deferred payments.
- [ ] Deliver orders with or without outstanding balance.
- [ ] Prevent duplicate delivery and duplicate payment submission.
- [ ] Include Dry Clean cash movements in shift reports.

### Phase 4: UI and Reporting

- [ ] Build dashboard, order form, order list, details, ready list, and delivery dialog.
- [ ] Build customer history and productivity reports.
- [ ] Add permissions and Arabic translations.

### Checkpoint: Complete

- [ ] `npx tsc --noEmit` passes.
- [ ] `npm run lint` passes.
- [ ] `npm test` passes.
- [ ] `npm run build` passes.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Removing old schema breaks existing data | High | Use a fresh Dry Clean database baseline and explicitly document data reset/migration policy |
| Productivity counted twice | High | Unique order productivity fact and transactionally idempotent completion |
| Delivery/payment race | High | Transactional state transition and duplicate guards |
| Employee/user identity confusion | Medium | Keep worker IDs separate from authenticated actor IDs |
| Receipt reprint creates financial side effects | Medium | Printing endpoints remain read-only with respect to order/payment state |

## Confirmed Decisions

- Customer address is snapshotted on the order and printed on the customer receipt/invoice.
- All operational workers come from `Employee`; authenticated users remain audit actors.
- Credit delivery is allowed without manager approval.
- Discounts remain available as an order-level amount until a more detailed rule is supplied.
