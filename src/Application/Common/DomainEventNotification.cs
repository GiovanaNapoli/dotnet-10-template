using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces.Common;
using MediatR;

namespace Application.Common
{
    public record DomainEventNotification<TEvent>(TEvent Event) : INotification
    where TEvent : IDomainEvent;
}