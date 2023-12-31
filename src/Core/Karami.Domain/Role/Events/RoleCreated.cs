﻿using Karami.Core.Domain.Attributes;
using Karami.Core.Domain.Constants;
using Karami.Core.Domain.Contracts.Abstracts;

namespace Karami.Domain.Role.Events;

[MessageBroker(Queue = Broker.Auth_Role_Queue)]
public class RoleCreated : CreateDomainEvent
{
    public string Id   { get; init; }
    public string Name { get; init; }
}