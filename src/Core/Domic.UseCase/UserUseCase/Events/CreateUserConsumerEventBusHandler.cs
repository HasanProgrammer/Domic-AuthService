﻿using Domic.Core.Common.ClassConsts;
using Domic.Core.Domain.Contracts.Interfaces;
using Domic.Core.Domain.Enumerations;
using Domic.Core.Domain.Extensions;
using Domic.Core.UseCase.Attributes;
using Domic.Core.UseCase.Contracts.Interfaces;
using Domic.Domain.PermissionUser.Contracts.Interfaces;
using Domic.Domain.PermissionUser.Entities;
using Domic.Domain.RoleUser.Contracts.Interfaces;
using Domic.Domain.RoleUser.Entities;
using Domic.Domain.User.Contracts.Interfaces;
using Domic.Domain.User.Entities;
using Domic.Domain.User.Events;

namespace Domic.UseCase.UserUseCase.Events;

public class CreateUserConsumerEventBusHandler : IConsumerEventBusHandler<UserCreated>
{
    private readonly IUserQueryRepository           _userQueryRepository;
    private readonly IRoleUserQueryRepository       _roleUserQueryRepository;
    private readonly IPermissionUserQueryRepository _permissionUserQueryRepository;
    private readonly IGlobalUniqueIdGenerator       _globalUniqueIdGenerator;

    public CreateUserConsumerEventBusHandler(IUserQueryRepository userQueryRepository, 
        IRoleUserQueryRepository roleUserQueryRepository, IPermissionUserQueryRepository permissionUserQueryRepository,
        IGlobalUniqueIdGenerator globalUniqueIdGenerator
    )
    {
        _userQueryRepository           = userQueryRepository;
        _roleUserQueryRepository       = roleUserQueryRepository;
        _permissionUserQueryRepository = permissionUserQueryRepository;
        _globalUniqueIdGenerator       = globalUniqueIdGenerator;
    }

    public void BeforeHandle(UserCreated @event){}

    [TransactionConfig(Type = TransactionType.Query)]
    public void Handle(UserCreated @event)
    {
        var targetUser = _userQueryRepository.FindByIdAsync(@event.Id, default).Result;

        if (targetUser is null) //Replication management
        {
            var newUser = new UserQuery {
                Id          = @event.Id                                 ,
                CreatedBy   = @event.CreatedBy                          ,
                CreatedRole = @event.CreatedRole                        , 
                FirstName   = @event.FirstName                          ,
                LastName    = @event.LastName                           ,
                Username    = @event.Username                           ,
                Password    = @event.Password.HashAsync(default).Result ,
                IsActive    = @event.IsActive ? IsActive.Active : IsActive.InActive ,
                CreatedAt_EnglishDate = @event.CreatedAt_EnglishDate,
                CreatedAt_PersianDate = @event.CreatedAt_PersianDate
            };

            _userQueryRepository.Add(newUser);
        
            _roleUserBuilder(@event.Id, @event.Roles, @event.CreatedBy, @event.CreatedRole, 
                @event.CreatedAt_EnglishDate, @event.CreatedAt_PersianDate
            );
            
            _permissionUserBuilder(@event.Id, @event.Permissions, @event.CreatedBy, @event.CreatedRole, 
                @event.CreatedAt_EnglishDate, @event.CreatedAt_PersianDate
            );
        }
    }

    public void AfterHandle(UserCreated @event){}

    /*---------------------------------------------------------------*/
    
    private void _roleUserBuilder(string userId, IEnumerable<string> roleIds, string createdBy, string createdRole,
        DateTime englishCreatedAt, string persianCreatedAt
    )
    {
        var roleUsers = _roleUserQueryRepository.FindAllByUserIdAsync(userId, default).Result;
        
        //1 . Remove already user roles
        _roleUserQueryRepository.RemoveRange(roleUsers);
        
        //2 . Assign new roles for user
        foreach (var roleId in roleIds)
        {
            var newRoleUser = new RoleUserQuery {
                Id          =_globalUniqueIdGenerator.GetRandom(),
                CreatedBy   = createdBy,
                CreatedRole = createdRole,
                UserId      = userId,
                RoleId      = roleId,
                CreatedAt_EnglishDate = englishCreatedAt,
                CreatedAt_PersianDate = persianCreatedAt
            };

            _roleUserQueryRepository.Add(newRoleUser);
        }
    }
    
    private void _permissionUserBuilder(string userId, IEnumerable<string> permissionIds, string createdBy, 
        string createdRole, DateTime englishCreatedAt, string persianCreatedAt
    )
    {
        var permissionUsers = _permissionUserQueryRepository.FindAllByUserIdAsync(userId, default).Result;
        
        //1 . Remove already user permissions
        _permissionUserQueryRepository.RemoveRange(permissionUsers);
        
        //2 . Assign new permissions for user
        foreach (var permissionId in permissionIds)
        {
            var newPermissionUser = new PermissionUserQuery {
                Id           = _globalUniqueIdGenerator.GetRandom(),
                CreatedBy    = createdBy,
                CreatedRole  = createdRole,
                UserId       = userId,
                PermissionId = permissionId,
                CreatedAt_EnglishDate = englishCreatedAt,
                CreatedAt_PersianDate = persianCreatedAt
            };

            _permissionUserQueryRepository.Add(newPermissionUser);
        }
    }
}