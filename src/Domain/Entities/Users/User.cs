using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Domain.Entities.Users.Events;

namespace Domain.Entities.Users
{
    public class User : BaseAuditableEntity
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public bool IsActive { get; set; } = true;

        public ImageFile? ProfilePicture { get; set; }

        private User() { }

        public static User Create(string name, string email, ImageFile? profilePicture = null)
        {
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                throw new ArgumentException("Invalid email format.", nameof(email));
            
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            
            var user = new User {Name = name, Email = email, ProfilePicture = profilePicture};
            user.AddDomainEvent(new UserCreatedEvent(user));
            return user;
        }

        public void Deactive()
        {
            if (!IsActive) return;
            IsActive = false;
            AddDomainEvent(new UserDeactivatedEvent(Id));
        }
    }
}