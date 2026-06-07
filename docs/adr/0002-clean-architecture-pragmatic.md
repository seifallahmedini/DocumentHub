# Pragmatic Clean Architecture layer structure

We organise the backend into four projects: Domain (no dependencies), Application (→ Domain), Infrastructure (→ Domain + Application), and Api (→ Application + Infrastructure for DI wiring, → Domain for read models). This enforces dependency inversion — business logic in Application never depends on EF Core or JWT libraries — while avoiding the overhead of mapping layers for simple query paths. The Api project is allowed to reference Domain directly for read-only endpoints to skip the Application use-case ceremony when no business logic is involved.

## Considered Options

Vertical Slice Architecture was considered. It is faster to scaffold but allows business logic to leak into endpoint handlers as the feature count grows, making cross-cutting concerns (auth, validation, tenant isolation) harder to enforce consistently.
