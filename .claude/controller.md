---
description: Use when creating Controllers or API endpoints. Applies to any new HTTP route.
globs: src/WebApi/Controllers/**
---

# Controller Endpoint

## File Structure

```
src/WebApi/Controllers/
  {Entity}sController.cs
```

## Rules

- Inherits from `ControllerBase` — NEVER `Controller`
- Always use `[ApiController]` and `[Route("api/[controller]")]`
- Inject ONLY `IMediator` — never repositories or services directly
- ALL logic stays in Handlers — controller only routes
- Always use `CancellationToken ct` in every endpoint
- Check `result.IsSuccess` to decide HTTP status
- Route parameters always typed: `{id:guid}`

## HTTP Status Mapping

| Situation | Status | Method |
|---|---|---|
| Success with data | 200 | `Ok(result)` |
| Created | 201 | `CreatedAtAction(...)` |
| Success no data | 204 | `NoContent()` |
| Validation/business failure | 400 | `BadRequest(result)` |
| Unauthenticated | 401 | `Unauthorized(result)` |
| Not found | 404 | `NotFound(result)` |

## Full Controller Template

```csharp
using Application.Features.{Entity}s.Commands.Create{Entity};
using Application.Features.{Entity}s.Commands.Update{Entity};
using Application.Features.{Entity}s.Commands.Delete{Entity};
using Application.Features.{Entity}s.Queries.Get{Entity}ById;
using Application.Features.{Entity}s.Queries.Get{Entity}s;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class {Entity}sController : ControllerBase
{
    private readonly IMediator _mediator;

    public {Entity}sController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new Get{Entity}sQuery(pageNumber, pageSize), ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new Get{Entity}ByIdQuery(id), ct);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] Create{Entity}Command command,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] Update{Entity}Command command,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(command with { Id = id }, ct);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new Delete{Entity}Command(id), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result);
    }
}
```

## Public endpoints inside authenticated controller

```csharp
[AllowAnonymous]
[HttpPost("register")]
public async Task<IActionResult> Register(...)
```
