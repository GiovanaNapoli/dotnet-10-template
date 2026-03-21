using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces.Common;

namespace Domain.Entities.Users.Events
{
    public record UserDeactivatedEvent(Guid UserId) : IDomainEvent;
}