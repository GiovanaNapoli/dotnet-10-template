---
description: Use when creating unit tests for any layer (Domain, Application, WebApi). Applies to any request to write tests, follow TDD, or validate behavior. Always use this skill before implementing a new feature — tests come first. Triggers on phrases like "criar testes", "escrever testes", "TDD", "testar", "unit test", "cobertura de testes", or when any other skill finishes implementing a feature.
---

# TDD Unit Tests — xUnit + Moq

## TDD Cycle (mandatory)

```
1. RED   → Write a failing test first
2. GREEN → Write minimum code to make it pass
3. REFACTOR → Clean up without breaking tests
```

Never write implementation before the test. If the user asks to implement a feature, write the tests first and confirm they fail before proceeding.

## Stack

- **xUnit** — test framework
- **Moq** — mocking library
- **Triple A pattern** — Arrange / Act / Assert (always use comments)

## Project Structure

```
tests/
  Domain.UnitTests/
    Entities/
      {Entity}Tests.cs
  Application.UnitTests/
    {Entity}s/
      {Action}{Entity}HandlerTests.cs
      {Action}{Entity}ValidatorTests.cs
  WebApi.UnitTests/
    Controllers/
      {Entity}sControllerTests.cs
```

## Naming Convention

```
Method_StateUnderTest_ExpectedBehavior

Examples:
  Create_WithValidParameters_ShouldReturnUser
  Handle_WhenEmailAlreadyExists_ShouldReturnFailure
  Handle_WhenIdentityServiceFails_ShouldReturnFailureAndRollback
  GetById_WhenUserNotFound_ShouldReturnNotFound
```

---

## Domain Tests — Entities

Test entity behavior, factory methods, domain rules, and domain events.

```csharp
using Domain.Entities.{Entity}s;
using Domain.Entities.{Entity}s.Events;
using Xunit;

namespace Domain.UnitTests.Entities;

public class {Entity}Tests
{
    // --- Factory Method ---

    [Fact]
    public void Create_WithValidParameters_ShouldReturn{Entity}()
    {
        // Arrange
        var name = "Valid Name";

        // Act
        var entity = {Entity}.Create(name);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(name, entity.Name);
        Assert.NotEqual(Guid.Empty, entity.Id);
        Assert.True(entity.IsActive);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => {Entity}.Create(name));
    }

    [Fact]
    public void Create_ShouldDispatch{Entity}CreatedEvent()
    {
        // Arrange & Act
        var entity = {Entity}.Create("Valid Name");

        // Assert
        Assert.Single(entity.DomainEvents);
        Assert.IsType<{Entity}CreatedEvent>(entity.DomainEvents.First());
    }

    // --- Business Methods ---

    [Fact]
    public void Deactivate_WhenActive_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var entity = {Entity}.Create("Valid Name");

        // Act
        entity.Deactivate();

        // Assert
        Assert.False(entity.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldNotChangeState()
    {
        // Arrange
        var entity = {Entity}.Create("Valid Name");
        entity.Deactivate();

        // Act
        entity.Deactivate();

        // Assert
        Assert.False(entity.IsActive);
    }
}
```

---

## Application Tests — Command Handlers

Test handler logic with mocked dependencies. Never test the pipeline (Behaviors) here — those are integration concerns.

```csharp
using Application.Common;
using Application.Features.{Entity}s.Commands.Create{Entity};
using Domain.Entities.{Entity}s;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Moq;
using Xunit;

namespace Application.UnitTests.{Entity}s;

public class Create{Entity}HandlerTests
{
    private readonly Create{Entity}CommandHandler _handler;
    private readonly Mock<I{Entity}Repository> _{entity}RepositoryMock;

    public Create{Entity}HandlerTests()
    {
        _{entity}RepositoryMock = new Mock<I{Entity}Repository>();

        _handler = new Create{Entity}CommandHandler(
            _{entity}RepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldReturnSuccess()
    {
        // Arrange
        var command = new Create{Entity}Command("Valid Name");

        _{entity}RepositoryMock
            .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
        _{entity}RepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<{Entity}>(), default),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNameAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new Create{Entity}Command("Existing Name");

        _{entity}RepositoryMock
            .Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        _{entity}RepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<{Entity}>(), default),
            Times.Never);
    }
}
```

---

## Application Tests — Validators

Test all validation rules in isolation.

```csharp
using Application.Features.{Entity}s.Commands.Create{Entity};
using FluentValidation.TestHelper;
using Xunit;

namespace Application.UnitTests.{Entity}s;

public class Create{Entity}ValidatorTests
{
    private readonly Create{Entity}CommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldHaveNoErrors()
    {
        // Arrange
        var command = new Create{Entity}Command("Valid Name");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyName_ShouldHaveValidationError(string? name)
    {
        // Arrange
        var command = new Create{Entity}Command(name!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var command = new Create{Entity}Command(new string('a', 101));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
```

---

## WebApi Tests — Controllers

Test HTTP responses using mocked IMediator.

```csharp
using Application.Common;
using Application.Features.{Entity}s.Commands.Create{Entity};
using Application.Features.{Entity}s.Queries.Get{Entity}ById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;
using Xunit;

namespace WebApi.UnitTests.Controllers;

public class {Entity}sControllerTests
{
    private readonly {Entity}sController _controller;
    private readonly Mock<IMediator> _mediatorMock;

    public {Entity}sControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new {Entity}sController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetById_WhenFound_ShouldReturnOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new {Entity}Dto(id, "Valid Name", DateTime.UtcNow);
        var response = ResponseBase<{Entity}Dto>.Success(dto);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<Get{Entity}ByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(id, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var response = ResponseBase<{Entity}Dto>.Failure("{Entity} not found.");

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<Get{Entity}ByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.GetById(id, default);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidCommand_ShouldReturnOk()
    {
        // Arrange
        var command = new Create{Entity}Command("Valid Name");
        var response = ResponseBase.Success();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<Create{Entity}Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(command, default);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
    }

    [Fact]
    public async Task Create_WhenHandlerFails_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new Create{Entity}Command("Existing Name");
        var response = ResponseBase.Failure("Name already exists.");

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<Create{Entity}Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Create(command, default);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequest.StatusCode);
    }
}
```

---

## What NOT to test

- `SaveChangesAsync` behavior — that's `UnitOfWorkBehavior` (integration concern)
- AutoMapper mappings in isolation — test them via handler tests
- EF Core queries — use integration tests for that
- Pipeline behaviors in unit tests — test them separately if needed

## Checklist before delivering tests

- [ ] Tests written BEFORE implementation (TDD)
- [ ] Failing test confirmed before writing code
- [ ] Triple A pattern with comments in every test
- [ ] Naming follows `Method_State_ExpectedBehavior`
- [ ] Only one assertion concept per test
- [ ] Mocks verify interactions when relevant (`Times.Once`, `Times.Never`)
- [ ] Edge cases covered (empty, null, boundary values)
- [ ] `[Theory]` + `[InlineData]` for multiple input scenarios
