using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Commands.UserCommands;

/// <summary>
/// 创建用户命令
/// </summary>
public record CreateUserCommand(string Name, string Email, string Password, string Phone, string RealName, int Status, string Gender, DateTimeOffset BirthDate, DeptId? DeptId, string? DeptName, IEnumerable<AssignAdminUserRoleQueryDto> RolesToBeAssigned) : ICommand<UserId>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(UserQuery userQuery)
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage("用户名不能为空");
        RuleFor(u => u.Password).NotEmpty().WithMessage("密码不能为空");
        RuleFor(u => u.Name).MustAsync(async (n, ct) => !await userQuery.DoesUserExist(n, ct))
            .WithMessage(u => $"该用户已存在，Name={u.Name}");
    }
}

/// <summary>
/// 创建用户命令处理器
/// </summary>
public class CreateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var passwordHash = Utils.PasswordHasher.HashPassword(request.Password);

        List<UserRole> roles = [];
        foreach (var assignAdminUserRoleDto in request.RolesToBeAssigned)
        {
            roles.Add(new UserRole(assignAdminUserRoleDto.RoleId, assignAdminUserRoleDto.RoleName));
        }

        var user = new User(request.Name, request.Phone, passwordHash, roles, request.RealName, request.Status, request.Email, request.Gender, request.BirthDate);

        // 分配部门
        if (request.DeptId != null && !string.IsNullOrEmpty(request.DeptName))
        {
            var dept = new UserDept(user.Id, request.DeptId, request.DeptName);
            user.AssignDept(dept);
        }

        await userRepository.AddAsync(user, cancellationToken);

        return user.Id;
    }
}

