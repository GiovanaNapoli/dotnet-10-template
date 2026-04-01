---
description: Use when creating Repositories or Repository Interfaces. Applies to any new data access layer for an entity.
globs: src/Infrastructure/Repositories/**,src/Domain/Interfaces/Repositories/**
---

# Repository + Interface

## File Structure

```
src/Domain/Interfaces/Repositories/
  I{Entity}Repository.cs

src/Infrastructure/Repositories/
  {Entity}Repository.cs

src/Infrastructure/Configurations/
  {Entity}Configuration.cs
```

## Rules

- Interface lives in Domain — ZERO infrastructure dependencies
- Implementation lives in Infrastructure
- Read queries use `.AsNoTracking()`
- Update/Delete do NOT use `.AsNoTracking()` — EF Core needs tracking
- NEVER call `SaveChangesAsync` in repository — UnitOfWorkBehavior handles it
- Auto-registered by convention in DependencyInjection — no manual registration needed
- Always create EF Core Configuration alongside the repository
- Always add `DbSet` in `ApplicationDbContext`

## Interface Template

```csharp
using Application.Common;
using Domain.Entities.{Entity}s;

namespace Domain.Interfaces.Repositories;

public interface I{Entity}Repository : IRepositoryBase<{Entity}>
{
    Task<{Entity}?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<PaginatedList<{Entity}>> Get{Entity}sAsync(
        int pageNumber, int pageSize, CancellationToken ct = default);
}
```

## Implementation Template

```csharp
using Application.Common;
using Domain.Entities.{Entity}s;
using Domain.Interfaces.Repositories;
using Infrastructure.Common;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class {Entity}Repository : RepositoryBase<{Entity}>, I{Entity}Repository
{
    public {Entity}Repository(ApplicationDbContext context) : base(context) { }

    public async Task<{Entity}?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Name == name, ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
        => await _dbSet.AnyAsync(e => e.Name == name, ct);

    public async Task<PaginatedList<{Entity}>> Get{Entity}sAsync(
        int pageNumber, int pageSize, CancellationToken ct = default)
        => await _dbSet
            .AsNoTracking()
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .ToPaginatedListAsync(pageNumber, pageSize, ct);
}
```

## EF Core Configuration Template

```csharp
using Domain.Entities.{Entity}s;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.ToTable("{Entity}s");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).HasMaxLength(100);
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);
    }
}
```

## ApplicationDbContext — add DbSet

```csharp
public new DbSet<{Entity}> {Entity}s { get; set; }
```

## After creating repository — run migration

```bash
dotnet ef migrations add Add{Entity} \
  --project src/Infrastructure \
  --startup-project src/WebApi \
  --output-dir Migrations
```
