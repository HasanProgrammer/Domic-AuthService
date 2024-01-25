using Karami.Core.Domain.Attributes;
using Karami.Core.Domain.Constants;
using Karami.Core.Domain.Contracts.Abstracts;

namespace Karami.Domain.Permission.Events;

[MessageBroker(Queue = Broker.Auth_Permission_Queue)]
public class PermissionCreated : CreateDomainEvent<string>
{
    public string RoleId { get; init; }
    public string Name   { get; init; }
}