using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task Handle_ShouldCreateUser()
        {
            // Arrange
            // Act
            // Assert
        }
    }
}