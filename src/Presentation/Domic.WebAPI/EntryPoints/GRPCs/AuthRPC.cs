﻿using Grpc.Core;
using Domic.Core.Auth.Grpc;
using Domic.Core.UseCase.Contracts.Interfaces;
using Domic.UseCase.UserUseCase.Commands.SignIn;
using Domic.WebAPI.Frameworks.Extensions.Mappers.UserMappers;

namespace Domic.WebAPI.EntryPoints.GRPCs;

public class AuthRPC : AuthService.AuthServiceBase
{
    private readonly IMediator      _mediator;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediator"></param>
    /// <param name="configuration"></param>
    public AuthRPC(IMediator mediator, IConfiguration configuration)
    {
        _mediator      = mediator;
        _configuration = configuration;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task<SignInResponse> SignIn(SignInRequest request, ServerCallContext context)
    {
        var command = request.ToCommand<SignInCommand>();
        
        var result = await _mediator.DispatchAsync<string>(command, context.CancellationToken);

        return result.ToRpcResponse<SignInResponse>(_configuration);
    }
}