---
description: Use when creating Domain Entities or Domain Events. Applies to any new business concept modeled in the domain.
globs: src/Domain/Entities/**
---

# Domain Entity + Domain Events

## File Structure

```
src/Domain/Entities/{Entity}s/
  {Entity}.cs
  Events/
    {Entity}CreatedEvent.cs
    {Entity}UpdatedEvent.cs
    {Entity}DeactivatedEvent.cs
src/Domain/Interfaces/Repositories/
  I{Entity}Repository.cs
```

## Rules

- Entity inherits from `BaseAuditableEntity`
- Constructor is ALWAYS `private`
- Instance ALWAYS via static factory method
- Factory methods dispatch domain events
- Properties have `private set` or `init` — NEVER public setter
- Business methods dispatch relevant domain events
- Domain Events are `record` implementing `IDomainEvent`
- NO infrastructure logic in entities (no EF Core, no repositories)

## Entity Template

```csharp
using Domain.Entities.Common;
using Domain.Entities.{Entity}s.Events;

namespace Domain.Entities.{Entity}s;

public class {Entity} : BaseAuditableEntity
{
    public required string Property { get; set; }
    public bool IsActive { get; set; } = true;

    private {Entity}() { }

    public static {Entity} Create(string property)
    {
        var entity = new {Entity} { Property = property };
        entity.AddDomainEvent(new {Entity}CreatedEvent(entity.Id, property));
        return entity;
    }

    public void Update(string property)
    {
        Property = property;
        AddDomainEvent(new {Entity}UpdatedEvent(Id, property));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new {Entity}DeactivatedEvent(Id));
    }
}
```

## Domain Event Template

```csharp
using Domain.Interfaces.Common;

namespace Domain.Entities.{Entity}s.Events;

public record {Entity}CreatedEvent(
    Guid {Entity}Id,
    string Property
) : IDomainEvent;
```

## Event Handler Template (Application layer)

```csharp
// src/Application/Features/{Entity}s/EventHandlers/{Entity}CreatedEventHandler.cs
using Application.Common;
using Domain.Entities.{Entity}s.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.{Entity}s.EventHandlers;

public class {Entity}CreatedEventHandler
    : INotificationHandler<DomainEventNotification<{Entity}CreatedEvent>>
{
    private readonly ILogger<{Entity}CreatedEventHandler> _logger;

    public {Entity}CreatedEventHandler(ILogger<{Entity}CreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        DomainEventNotification<{Entity}CreatedEvent> notification,
        CancellationToken ct)
    {
        _logger.LogInformation("{Entity} created: {Id}", nameof({Entity}), notification.Event.{Entity}Id);
        return Task.CompletedTask;
    }
}
```

## Domain Event Flow

```
Entity.Create()
  → AddDomainEvent(XCreatedEvent)       ← Domain, no framework

SaveChangesAsync() in DbContext
  → collect DomainEvents                ← Infrastructure
  → DomainEventDispatcher.DispatchAsync()
  → IMediator.Publish()

XCreatedEventHandler.Handle()           ← Application reacts
```

## TDD — Write tests first

Before implementing the entity, create the test file in `tests/Domain.UnitTests/Entities/`:

```
1. RED   → Write {Entity}Tests with failing assertions
2. GREEN → Implement the Entity to make tests pass
3. REFACTOR → Clean up
```

See `tests.md` for full test templates.
