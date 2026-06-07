# Ardalis Clean Architecture project structure

We organise the backend following the [Ardalis Clean Architecture](https://github.com/ardalis/CleanArchitecture) template conventions, using three source projects and three test projects:

```
src/
  DocumentHub.Core/          ← entities, exceptions, interfaces, use-case handlers
  DocumentHub.Infrastructure/ ← EF Core, auth adapters, DI wiring
  DocumentHub.Web/           ← ASP.NET Core host, endpoint classes, thin Program.cs

tests/
  DocumentHub.UnitTests/        ← fast, no I/O; handler logic, value objects
  DocumentHub.IntegrationTests/ ← WebApplicationFactory end-to-end tests
  DocumentHub.FunctionalTests/  ← browser / full-stack tests (placeholder)
```

**Dependency rule** — inward only:

```
Web → Core, Infrastructure
Infrastructure → Core
Core → (nothing, except Microsoft.Extensions.DependencyInjection.Abstractions for DI extensions)
```

Business logic in `Core` never depends on EF Core, JWT, or any I/O library. Infrastructure adapters implement Core interfaces and stay confined to the `Infrastructure` project.

**Endpoint organisation** — each API route lives in its own static class under `Web/Endpoints/<Feature>/`. `Program.cs` maps endpoint groups or individual endpoint classes; it contains no inline request-handling logic.

**Why Core is one project, not two** — the Ardalis template merges what some architectures split into Domain + Application into a single `Core` project. This avoids artificial seams between entities and the use cases that operate on them, while still preventing infrastructure and framework leakage. The dependency rule above preserves the same inward-only guarantee.

## Supersedes

The original ADR-0002 described a four-project layout (Domain / Application / Infrastructure / Api). That layout was replaced in favour of the Ardalis naming and structure because Ardalis is a widely recognised .NET reference and reduces boilerplate project-crossing for small to medium feature sets.

## Considered Options

**Vertical Slice Architecture** was considered. It is faster to scaffold but allows business logic to leak into endpoint handlers as the feature count grows, making cross-cutting concerns (auth, validation, tenant isolation) harder to enforce consistently.

**Keep four-project layout** was the prior decision. It was superseded because the Domain/Application split added project-crossing ceremony without meaningful isolation benefit at current scale — the Ardalis Core project achieves the same dependency inversion with less indirection.
