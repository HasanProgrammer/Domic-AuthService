﻿using Domic.Core.Common.ClassConsts;
using Domic.Core.Domain.Enumerations;
using Domic.Core.UseCase.Attributes;
using Domic.Core.UseCase.Contracts.Interfaces;
using Domic.Domain.Permission.Contracts.Interfaces;
using Domic.Domain.Permission.Events;
using Domic.Domain.PermissionUser.Contracts.Interfaces;

namespace Domic.UseCase.PermissionUseCase.Events;

public class DeletePermissionConsumerEventBus : IConsumerEventBusHandler<PermissionDeleted>
{
    private readonly IPermissionQueryRepository     _permissionQueryRepository;
    private readonly IPermissionUserQueryRepository _permissionUserQueryRepository;

    public DeletePermissionConsumerEventBus(IPermissionQueryRepository permissionQueryRepository, 
        IPermissionUserQueryRepository permissionUserQueryRepository
    )
    {
        _permissionQueryRepository     = permissionQueryRepository;
        _permissionUserQueryRepository = permissionUserQueryRepository;
    }

    public void BeforeHandle(PermissionDeleted @event){}

    [TransactionConfig(Type = TransactionType.Query)]
    public void Handle(PermissionDeleted @event)
    {
        var targetPermission = _permissionQueryRepository.FindByIdAsync(@event.Id, default).Result;
            
        if (targetPermission is not null) //Replication management
        {
            #region SoftDeletePermission

            targetPermission.UpdatedBy   = @event.UpdatedBy;
            targetPermission.UpdatedRole = @event.UpdatedRole;
            targetPermission.IsDeleted   = IsDeleted.Delete;
            targetPermission.UpdatedAt_EnglishDate = @event.UpdatedAt_EnglishDate;
            targetPermission.UpdatedAt_PersianDate = @event.UpdatedAt_PersianDate;
        
            _permissionQueryRepository.Change(targetPermission);

            #endregion

            #region HardDelete PermissionUser

            var permissionUsers = _permissionUserQueryRepository.FindAllByPermissionIdAsync(@event.Id, default).Result;

            _permissionUserQueryRepository.RemoveRange(permissionUsers);

            #endregion
        }
    }

    public void AfterHandle(PermissionDeleted @event){}
}