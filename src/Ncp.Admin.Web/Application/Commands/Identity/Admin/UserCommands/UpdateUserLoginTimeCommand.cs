using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

public record UpdateUserLoginTimeCommand(UserId UserId, DateTimeOffset LoginTime, string RefreshToken) : ICommand;

public class UpdateUserLoginTimeCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateUserLoginTimeCommand>
{
    public async Task Handle(UpdateUserLoginTimeCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetAsync(request.UserId, cancellationToken)
                   ?? throw new KnownException($"未找到用户，UserId = {request.UserId}");

        user.UpdateLastLoginTime(request.LoginTime);
        user.SetUserRefreshToken(request.RefreshToken);
    }
}

