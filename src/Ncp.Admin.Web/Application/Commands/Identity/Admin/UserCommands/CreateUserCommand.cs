using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Queries;
using Serilog;

namespace Ncp.Admin.Web.Application.Commands.Identity.Admin.UserCommands;

/// <summary>
/// 创建用户命令
/// </summary>
public record CreateUserCommand(
    string Name,
    string Email,
    string Password,
    string Phone,
    string RealName,
    int Status,
    string Gender,
    DateTimeOffset BirthDate,
    DeptId? DeptId,
    string? DeptName,
    bool IsDeptManager,
    PositionId? PositionId,
    string? PositionName,
    IEnumerable<AssignAdminUserRoleQueryDto> RolesToBeAssigned,
    UserId CreatorId,
    string IdCardNumber,
    string Address,
    string Education,
    string GraduateSchool,
    string AvatarUrl,
    bool NotOrderMeal,
    string WechatGuid,
    bool IsResigned,
    DateTimeOffset ResignedTime) : ICommand<UserId>;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator(UserQuery userQuery)
    {
        RuleFor(u => u.Name).NotEmpty().WithMessage("用户名不能为空");
        RuleFor(u => u.Password).NotEmpty().WithMessage("密码不能为空");
        When(u => u.IsResigned, () =>
        {
            RuleFor(u => u.ResignedTime).NotNull().WithMessage("离职时间不能为空");
        });
        RuleFor(u => u.Name).MustAsync(async (n, ct) => !await userQuery.DoesUserExist(n, ct))
            .WithMessage(u => $"该用户已存在，Name={u.Name}");
    }
}

public class CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher) : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        Log.Information("开始创建用户: {UserName}, Email: {Email}", request.Name, request.Email);

        var passwordHash = passwordHasher.Hash(request.Password);

        var roles = request.RolesToBeAssigned
            .Select(r => new UserRole(r.RoleId, r.RoleName))
            .ToList();

        var user = new User(
            request.Name,
            request.Phone,
            passwordHash,
            roles,
            request.RealName,
            request.Status,
            request.Email,
            request.Gender,
            request.BirthDate,
            request.CreatorId,
            request.IdCardNumber,
            request.Address,
            request.Education,
            request.GraduateSchool,
            request.AvatarUrl,
            request.NotOrderMeal,
            request.WechatGuid,
            request.IsResigned,
            request.ResignedTime);

        await userRepository.AddAsync(user, cancellationToken);

        if (request.DeptId != null && !string.IsNullOrEmpty(request.DeptName))
        {
            user.AssignDept(request.DeptId, request.DeptName, request.IsDeptManager);
            Log.Debug("为用户分配部门: UserId={UserId}, DeptId={DeptId}, DeptName={DeptName}", user.Id, request.DeptId, request.DeptName);
        }

        if (request.PositionId != null && !string.IsNullOrEmpty(request.PositionName))
        {
            var position = new UserPosition(user.Id, request.PositionId!, request.PositionName);
            user.AssignPosition(position);
            Log.Debug("为用户分配岗位: UserId={UserId}, PositionId={PositionId}, PositionName={PositionName}", user.Id, request.PositionId, request.PositionName);
        }

        Log.Information("用户创建成功: UserId={UserId}, UserName={UserName}, Email={Email}, RoleCount={RoleCount}", 
            user.Id, request.Name, request.Email, roles.Count);

        return user.Id;
    }
}

