using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Users;
using Application.Features.Users.Commands.CreateUser;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Moq;
using Xunit;

namespace Application.UnitTests.Users
{
    public class CreateUserHandlerTests
    {
        private readonly CreateUserCommandHandler _handler;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IIdentityService> _identityServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        public CreateUserHandlerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _identityServiceMock = new Mock<IIdentityService>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateUserCommandHandler(_userRepositoryMock.Object, _identityServiceMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserIsCreated()
        {
            // Arrange
            var command = new CreateUserCommand(
                new CreateUserDto
                {
                    Name = "Test User",
                    Email = "test@example.com",
                    Password = "Password123!"
                });
            _userRepositoryMock
                .Setup(repo => repo.ExistsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _identityServiceMock
                .Setup(service => service.RegisterAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>()))
                .ReturnsAsync((true, (string?)null));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Contains("User created successfully.", result.Messages);
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Users.User>(), default), Times.Once);
            _identityServiceMock.Verify(service => service.RegisterAsync(command.User.Email, command.User.Password, It.IsAny<Guid>()), Times.Once);

        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenEmailAlreadyExists()
        {
            // Arrange
            var command = new CreateUserCommand(
                new CreateUserDto
                {
                    Name = "Test User",
                    Email = "test@example.com",
                    Password = "Password123!"
                });
            _userRepositoryMock
                .Setup(repo => repo.ExistsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Email already in use.", result.Errors.Select(e => e));
            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Users.User>(), default), Times.Never);
            _identityServiceMock.Verify(service => service.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailure_WhenIdentityServiceFails()
        {
            // Arrange
            var command = new CreateUserCommand(
                new CreateUserDto
                {
                    Name = "Test User",
                    Email = "test@example.com",
                    Password = "Password123!"
                });
            _userRepositoryMock
                .Setup(repo => repo.ExistsByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            _identityServiceMock
                .Setup(service => service.RegisterAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Guid>()))
                .ReturnsAsync((false, "Identity service error"));

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Identity service error", result.Errors.Select(e => e));

            _userRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.Users.User>(), default), Times.Once);    // ← Once, não Never
            _userRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Domain.Entities.Users.User>(), default), Times.Once); // ← Verifica o rollback
            _identityServiceMock.Verify(service => service.RegisterAsync(command.User.Email, command.User.Password, It.IsAny<Guid>()), Times.Once);
        }

    }
}