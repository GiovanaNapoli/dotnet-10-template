using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Users;
using Xunit;

namespace Domain.UnitTests.Entities
{
    public class UserTests
    {
        [Fact]
        public void Create_with_valid_parameters()
        {
            // Arrange
            var name = "John Doe";
            var email = "john.doe@email.com";

            // Act
            var user = User.Create(name, email);
            // Assert
            Assert.NotNull(user);
            Assert.Equal(name, user.Name);
            Assert.Equal(email, user.Email);
            Assert.NotEqual(Guid.Empty, user.Id);
            Assert.True(user.IsActive);
        }

        [Fact]
        public void Create_with_invalid_email_should_throw_exception()
        {
            // Arrange
            var name = "John Doe";
            var email = "invalid-email";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => User.Create(name, email));
        }

        [Fact]
        public void Create_with_empty_name_should_throw_exception()
        {
            // Arrange
            var name = "";
            var email = "john.doe@email.com";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => User.Create(name, email));
        }

        [Fact]
        public void Deactive_should_set_is_active_to_false()
        {
            // Arrange
            var user = User.Create("John Doe", "john.doe@email.com");

            // Act
            user.Deactive();

            // Assert
            Assert.False(user.IsActive);
        }

        [Fact]
        public void Deactive_when_already_inactive_should_not_change_state()
        {
            // Arrange
            var user = User.Create("John Doe", "john.doe@email.com");

            // Act
            user.Deactive();
            user.Deactive();

            // Assert
            Assert.False(user.IsActive);
        }
    }
}