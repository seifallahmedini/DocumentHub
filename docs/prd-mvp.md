## Problem Statement

Companies need a single place to store, find, and share internal documents — and to issue invoices to their clients. Today they rely on scattered file systems, email attachments, and manual spreadsheet-based invoicing, which makes retrieval slow, version control nonexistent, and billing error-prone.

## Solution

DocumentHub is a multi-tenant SaaS product where each company (Tenant) gets an isolated workspace. Users within a Tenant can upload and tag Documents for fast retrieval, manage a Client list, and create Invoices with Line Items and automatic 19% TVA calculation, then export them as PDFs.

## User Stories

### Authentication & Tenant Management

1. As a company owner, I want to register my company as a Tenant with my email and password, so that my team has an isolated workspace in DocumentHub.
2. As an Admin, I want to log in with my email and password, so that I can access my Tenant's workspace.
3. As a Member, I want to log in with my email and password, so that I can access Documents and Invoices.
4. As any User, I want to log out, so that my session is ended securely.
5. As an Admin, I want to invite a new Member by email, so that my colleagues can access the workspace.
6. As an Admin, I want to remove a Member from the Tenant, so that former employees lose access.
7. As an Admin, I want to view a list of all Users in my Tenant, so that I can manage access.
8. As any User, I want to be redirected to login when my session expires, so that I am never silently unauthenticated.

### Document Management

9. As a Member, I want to upload a Document of any file type, so that it is stored securely in my Tenant.
10. As a Member, I want to give a Document a name when uploading, so that it is identifiable without opening it.
11. As a Member, I want to attach one or more Tags to a Document, so that I can categorise it for later retrieval.
12. As a Member, I want to edit a Document's name and Tags after upload, so that I can correct mistakes.
13. As a Member, I want to delete a Document, so that outdated files are removed from the workspace.
14. As a Member, I want to download a Document, so that I can view or share the original file.
15. As a Member, I want to see a list of all Documents in my Tenant, so that I can browse what has been uploaded.
16. As a Member, I want to filter Documents by Tag, so that I can find all files in a category quickly.
17. As a Member, I want to search Documents by name, so that I can find a specific file without browsing.
18. As a Member, I want to see who uploaded a Document and when, so that I have an audit trail.

### Client Management

19. As a Member, I want to create a Client with a name, address, and email, so that I can issue Invoices to them.
20. As a Member, I want to edit a Client's details, so that I can keep contact information up to date.
21. As a Member, I want to delete a Client, so that inactive clients are removed from the workspace.
22. As a Member, I want to view a list of all Clients in my Tenant, so that I can select one when creating an Invoice.

### Invoicing

23. As a Member, I want to create an Invoice addressed to a Client, so that I can bill them for work done.
24. As a Member, I want to add Line Items to an Invoice with a description, quantity, and unit price, so that the Invoice reflects the work delivered.
25. As a Member, I want to see the Line Item total (quantity x unit price) calculated automatically, so that I do not make arithmetic errors.
26. As a Member, I want to see the Invoice subtotal and 19% TVA calculated automatically, so that the tax amount is always correct.
27. As a Member, I want to set an issue date and due date on an Invoice, so that payment terms are clear.
28. As a Member, I want Invoices to be assigned a sequential invoice number automatically, so that I never have duplicates.
29. As a Member, I want to save an Invoice as a Draft, so that I can review it before sending.
30. As a Member, I want to mark an Invoice as Sent, so that I can track that the Client has been billed.
31. As a Member, I want to mark an Invoice as Paid, so that I know which Invoices have been settled.
32. As a Member, I want to mark an Invoice as Cancelled, so that voided Invoices are clearly distinguished.
33. As a Member, I want Overdue Invoices to be flagged automatically when the due date passes unpaid, so that I can follow up with Clients.
34. As a Member, I want to view a list of all Invoices in my Tenant filtered by status, so that I can see what is outstanding.
35. As a Member, I want to export an Invoice as a PDF, so that I can send it to the Client by email.

## Implementation Decisions

- **Multi-tenancy**: Every database entity carries a TenantId. The API enforces tenant isolation at the data layer. TenantId is resolved from the authenticated User's JWT claims.
- **Authentication**: JWT bearer tokens issued by the API on login. Tokens carry UserId, TenantId, and Role (Admin / Member). No external identity provider for the MVP.
- **Invoice numbering**: Sequential per Tenant, formatted as INV-{YYYY}-{NNN} (e.g. INV-2026-001). The sequence resets each calendar year.
- **TVA**: Fixed at 19%. Applied to the Invoice subtotal (sum of all Line Item totals). Stored as a computed value on the Invoice, not recalculated on every read.
- **Document storage**: Files stored on the local filesystem in an uploads folder scoped by TenantId. The path is stored in the database and served through an authenticated API endpoint. No cloud storage for the MVP.
- **PDF generation**: Invoices rendered to PDF on demand using a server-side library (QuestPDF). PDFs are not stored -- generated at download time.
- **Database**: SQLite via Entity Framework Core. One database file for the whole application. See ADR-0001.
- **API**: .NET 10 Minimal API. All endpoints require authentication except Tenant registration and login.
- **Frontend**: React 19 + TypeScript + Vite. Auth state via React context; server state via fetch + useState for the MVP.
- **Testing seams**:
  - Backend: WebApplicationFactory integration tests with a real in-memory SQLite database.
  - Frontend: React Testing Library component tests with the fetch layer mocked at the boundary.

## Testing Decisions

A good test asserts on observable external behavior -- HTTP response contents, DOM output -- not on implementation details like which EF Core method was called.

**Backend** (tests/DocumentHub.Api.Tests/): One integration test class per feature area (Auth, Documents, Invoices, Clients). Each test uses a fresh WebApplicationFactory with an isolated SQLite database. Covers: happy path, tenant isolation, validation errors, status transition guards.

**Frontend** (src/documenthub-web/): React Testing Library tests per major form and list component. Fetch mocked at the window.fetch boundary using vi.fn() (Vitest). Covers: successful submission, error states, empty states.

## Out of Scope

- Email delivery (invitation emails, sending invoices to Clients).
- Subscription billing or payment processing for DocumentHub itself.
- Document version history.
- Role permissions more granular than Admin / Member.
- Cloud file storage (S3, Azure Blob).
- Two-factor authentication.
- Audit log UI.
- Mobile app or PWA.

## Further Notes

- All monetary amounts are in Tunisian Dinar (TND).
- Invoice number format INV-{YYYY}-{NNN} must be used consistently across the codebase and UI.
- The 19% TVA rate is fixed and must be a named constant, not a magic number.
- Domain vocabulary is defined in CONTEXT-MAP.md -- use it throughout the codebase.
