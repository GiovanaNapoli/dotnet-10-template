---
description: Use when creating Commands, CommandHandlers, or Validators. Applies to any write operation (create, update, delete).
globs: src/Application/Features/**/Commands/**
---

# Command + Handler + Validator

## File Structure

```
src/Application/Features/{Entity}s/Commands/{Action}{Entity}/
  {Action}{Entity}Command.cs
  {Action}{Entity}CommandHandler.cs
  {Action}{Entity}CommandValidator.cs
```

## Rules

- Command is always a `record` implementing `IRequest<ResponseBase>` or `IRequest<ResponseBase<TDto>>`
- Handler implements `IRequestHandler<TCommand, TResponse>`
- Validator extends `AbstractValidator<TCommand>`
- Handler NEVER injects `IUnitOfWork` — `UnitOfWorkBehavior` handles persistence
- Handler NEVER calls `SaveChangesAsync`
- Entity ALWAYS created via static factory method (`Entity.Create(...)`)
- Validator checks format/required — business rules go in the Handler

## Command Template

```csharp
using Application.Common;
using MediatR;

namespace Application.Features.{Entity}s.Commands.{Action}{Entity};

public record {Action}{Entity}Command(
    string Property
) : IRequest<ResponseBase>;
```

## Handler Template

```csharp
using Application.Common;
using Domain.Entities.{Entity}s;
using Domain.Interfaces.Repositories;
using MediatR;

namespace Application.Features.{Entity}s.Commands.{Action}{Entity};

public class {Action}{Entity}CommandHandler : IRequestHandler<{Action}{Entity}Command, ResponseBase>
{
    private readonly I{Entity}Repository _{entity}Repository;

    public {Action}{Entity}CommandHandler(I{Entity}Repository {entity}Repository)
    {
        _{entity}Repository = {entity}Repository;
    }

    public async Task<ResponseBase> Handle({Action}{Entity}Command cmd, CancellationToken ct)
    {
        // 1. Business rule validations
        // 2. Fetch entity if needed
        // 3. Execute operation via entity method
        // 4. Persist via repository (NO SaveChanges here)
        // 5. Return ResponseBase.Success() or ResponseBase.Failure()

        var entity = {Entity}.Create(cmd.Property);
        await _{entity}Repository.AddAsync(entity, ct);

        return ResponseBase.Success();
    }
}
```

## Validator Template

```csharp
using FluentValidation;

namespace Application.Features.{Entity}s.Commands.{Action}{Entity};

public class {Action}{Entity}CommandValidator : AbstractValidator<{Action}{Entity}Command>
{
    public {Action}{Entity}CommandValidator()
    {
        RuleFor(x => x.Property)
            .NotEmpty().WithMessage("Property is required.")
            .MaximumLength(100).WithMessage("Property must not exceed 100 characters.");
    }
}
```

## Pipeline Order (automatic)

```
AuditBehavior → ValidationBehavior → UnitOfWorkBehavior → Handler
```
