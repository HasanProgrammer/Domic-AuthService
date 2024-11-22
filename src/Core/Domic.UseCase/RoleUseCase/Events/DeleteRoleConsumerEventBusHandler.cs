﻿using Domic.Core.Common.ClassConsts;
using Domic.Core.Domain.Enumerations;
using Domic.Core.UseCase.Attributes;
using Domic.Core.UseCase.Contracts.Interfaces;
using Domic.Domain.Permission.Contracts.Interfaces;
using Domic.Domain.PermissionUser.Contracts.Interfaces;
using Domic.Domain.Role.Contracts.Interfaces;
using Domic.Domain.Role.Events;
using Domic.Domain.RoleUser.Contracts.Interfaces;

namespace Domic.UseCase.RoleUseCase.Events;

public class DeleteRoleConsumerEventBusHandler : IConsumerEventBusHandler<RoleDeleted>
{
    private readonly IRoleQueryRepository           _roleQueryRepository;
    private readonly IPermissionQueryRepository     _permissionQueryRepository;
    private readonly IPermissionUserQueryRepository _permissionUserQueryRepository;
    private readonly IRoleUserQueryRepository       _roleUserQueryRepository;

    public DeleteRoleConsumerEventBusHandler(IRoleQueryRepository roleQueryRepository, 
        IPermissionQueryRepository permissionQueryRepository, IRoleUserQueryRepository roleUserQueryRepository,
        IPermissionUserQueryRepository permissionUserQueryRepository
    )
    {
        _roleQueryRepository           = roleQueryRepository;
        _permissionQueryRepository     = permissionQueryRepository;
        _permissionUserQueryRepository = permissionUserQueryRepository;
        _roleUserQueryRepository       = roleUserQueryRepository;
    }

    public void BeforeHandle(RoleDeleted @event){}

    [TransactionConfig(Type = TransactionType.Query)]
    public void Handle(RoleDeleted @event)
    {
        var targetRole = _roleQueryRepository.FindByIdEagerLoadingAsync(@event.Id, default).Result;
            
        if (targetRole is not null) //Replication management
        {
            #region SoftDeleteRole

            targetRole.UpdatedBy   = @event.UpdatedBy;
            targetRole.UpdatedRole = @event.UpdatedRole;
            targetRole.IsDeleted   = IsDeleted.Delete;
            targetRole.UpdatedAt_EnglishDate = @event.UpdatedAt_EnglishDate;
            targetRole.UpdatedAt_PersianDate = @event.UpdatedAt_PersianDate;
        
            _roleQueryRepository.Change(targetRole);

            #endregion
        
            #region SoftDeletePermission

            foreach (var permission in targetRole.Permissions)
            {
                permission.UpdatedBy   = @event.UpdatedBy;
                permission.UpdatedRole = @event.UpdatedRole;
                permission.IsDeleted   = IsDeleted.Delete;
                permission.UpdatedAt_EnglishDate = @event.UpdatedAt_EnglishDate;
                permission.UpdatedAt_PersianDate = @event.UpdatedAt_PersianDate;
            
                _permissionQueryRepository.Change(permission);
            }

            #endregion
        
            #region HardDeleteRoleUser

            var roleUsers = _roleUserQueryRepository.FindAllByRoleIdAsync(@event.Id, default).Result;
        
            _roleUserQueryRepository.RemoveRange(roleUsers);

            #endregion
        
            #region HardDeletePermissionUser

            foreach (var permission in targetRole.Permissions)
            {
                var permissionUsers =
                    _permissionUserQueryRepository.FindAllByPermissionIdAsync(permission.Id, default).Result;
        
                _permissionUserQueryRepository.RemoveRange(permissionUsers);
            }

            #endregion
        }
    }

    public void AfterHandle(RoleDeleted @event){}
}