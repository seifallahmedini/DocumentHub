# Context Map

## Contexts

- [API](./src/DocumentHub.Api/CONTEXT.md) — backend domain: tenants, documents, invoices, storage
- [Web](./src/documenthub-web/CONTEXT.md) — frontend domain: UI flows, view models, user interactions

## Relationships

- Both contexts share the core domain language defined below
- API owns all data; Web reads and writes through the API

## Shared Domain Language

**DocumentHub**:
A SaaS product that provides document management and invoicing for companies. Built by TitanCore SUARL.
_Avoid_: TitanCoreHub, the app, the platform

**Tenant**:
A company that uses DocumentHub. The unit of data isolation — all documents, invoices, and users belong to exactly one Tenant.
_Avoid_: Company, organisation, account, workspace, client

**User**:
A person who belongs to exactly one Tenant and can authenticate into DocumentHub.
_Avoid_: Account, person, member (as a generic term)

**Admin**:
A User who manages the Tenant's settings, users, and subscription. Every Tenant has at least one Admin.
_Avoid_: Owner, super-user, manager

**Member**:
A User who can upload documents and create invoices but cannot manage Tenant settings or other users.
_Avoid_: Employee, regular user, contributor

**Document**:
A file of any type uploaded by a User and stored within a Tenant. Carries metadata (name, tags, uploader, upload date) but its contents are never parsed by DocumentHub.
_Avoid_: File, attachment, asset

**Tag**:
A free-form text label attached to a Document by a User. Used to filter and find Documents. Case-insensitive, no spaces.
_Avoid_: Category, label, keyword

**Client**:
An external party (person or company) that a Tenant issues Invoices to. Not a User — cannot log into DocumentHub. Exists only as contact data (name, address, email) within a Tenant.
_Avoid_: Customer, recipient, contact, account

**Invoice**:
A billing document issued by a Tenant to a Client. Carries an invoice number, issue date, due date, status, one or more Line Items, and a TVA amount applied to the subtotal.
_Avoid_: Bill, quote, order, receipt

**Line Item**:
A single chargeable entry on an Invoice. Has a description (free text), quantity, and unit price in TND. Line total = quantity × unit price.
_Avoid_: Row, entry, charge, product

**TVA**:
The Tunisian value-added tax applied at a fixed rate of 19% to the Invoice subtotal (sum of all Line Item totals). Displayed separately on the Invoice.
_Avoid_: VAT, tax, fee

**Invoice Status**:
The lifecycle state of an Invoice. One of: Draft (being prepared, not yet sent), Sent (delivered to the Client), Paid (payment confirmed), Overdue (past due date and unpaid), Cancelled (voided).
_Avoid_: State, stage, flag
