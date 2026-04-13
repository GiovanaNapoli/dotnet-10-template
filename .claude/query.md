---
description: Use when creating Queries, QueryHandlers, or DTOs. Applies to any read operation (get, list, search).
globs: src/Application/Features/**/Queries/**
---

# Query + Handler + DTO

## File Structure

```
src/Application/Features/{Entity}s/Queries/{Action}{Entity}/
  {Action}{Entity}Query.cs
  {Action}{Entity}QueryHandler.cs
src/Application/DTOs/
  {Entity}Dto.cs
```

## Rules

- Query is always a `record` implementing `IRequest<ResponseBase<TDto>>`
- Handler implements `IRequestHandler<TQuery, ResponseBase<TDto>>`
- DTO is always a `record` — immutable
- Queries NEVER modify state — read only
- DTOs NEVER reference Domain types directly — use primitives or other DTOs
- Handler uses AutoMapper to map Entity → DTO
- Paginaged lists return `ResponseBase<PaginatedList<TDto>>`
- Add mapping in `DomainToDTOMappingProfile`

## Query by Id Template

```csharp
using Application.Common;
using MediatR;

namespace Application.Features.{Entity}s.Queries.Get{Entity}ById;

public record Get{Entity}ByIdQuery(Guid Id) : IRequest<ResponseBase<{Entity}Dto>>;

public record {Entity}Dto(
    Guid Id,
    string Property,
    DateTime CreatedAt
);
```

## Handler by Id Template

```csharp
using Application.Common;
using AutoMapper;
using Domain.Interfaces.Repositories;
using MediatR;

namespace Application.Features.{Entity}s.Queries.Get{Entity}ById;

public class Get{Entity}ByIdQueryHandler
    : IRequestHandler<Get{Entity}ByIdQuery, ResponseBase<{Entity}Dto>>
{
    private readonly I{Entity}Repository _{entity}Repository;
    private readonly IMapper _mapper;

    public Get{Entity}ByIdQueryHandler(I{Entity}Repository {entity}Repository, IMapper mapper)
    {
        _{entity}Repository = {entity}Repository;
        _mapper = mapper;
    }

    public async Task<ResponseBase<{Entity}Dto>> Handle(
        Get{Entity}ByIdQuery query, CancellationToken ct)
    {
        var entity = await _{entity}Repository.GetByIdAsync(query.Id, ct);

        if (entity is null)
            return ResponseBase<{Entity}Dto>.Failure("{Entity} not found.");

        return ResponseBase<{Entity}Dto>.Success(_mapper.Map<{Entity}Dto>(entity));
    }
}
```

## Paginated Query Template

```csharp
public record Get{Entity}sQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<ResponseBase<PaginatedList<{Entity}Dto>>>;
```

## AutoMapper — add to DomainToDTOMappingProfile

```csharp
CreateMap<{Entity}, {Entity}Dto>();
```

## TDD — Write tests first

Before implementing, create the test file in `tests/Application.UnitTests/{Entity}s/`:

```
1. RED   → Write Get{Entity}HandlerTests with failing assertions
2. GREEN → Implement the Handler to make tests pass
3. REFACTOR → Clean up
```

See `tests.md` for full test templates.
