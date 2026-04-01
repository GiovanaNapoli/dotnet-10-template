# Clean Architecture .NET 10 — Project Conventions

This project follows Clean Architecture with CQRS using MediatR, EF Core, ASP.NET Identity, and JWT.

## Project Structure

```
src/
  Domain/           # Entities, Domain Events, Interfaces
  Application/      # Commands, Queries, Handlers, Validators, DTOs
  Infrastructure/   # EF Core, Repositories, Identity, JWT, Email
  WebApi/           # Controllers, DependencyInjection, Program.cs
tests/
  Domain.UnitTests/
  Application.UnitTests/
```

## Core Rules

- Domain has ZERO framework dependencies
- Application knows only Domain interfaces — never Infrastructure
- Infrastructure implements Domain interfaces
- WebApi only injects IMediator in controllers — never repositories or services directly
- Handlers NEVER call SaveChangesAsync — UnitOfWorkBehavior handles persistence automatically
- Handlers NEVER inject IUnitOfWork (unless explicit transaction is needed)
- Entities are ALWAYS created via static factory methods
- Entity constructors are ALWAYS private

## Detailed conventions per artifact: see .cursor/rules/ folder
