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
    DateTimeOffset ResignedTime,
    bool AttendanceRequired = true,
    bool SetAsDeptResponsibleUser = false,
    bool SetAsDefaultDeptResponsibleUser = false) : ICommand<UserId>;

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

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : ICommandHandler<CreateUserCommand, UserId>
{
    public async Task<UserId> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
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
            request.ResignedTime,
            request.AttendanceRequired);

        await userRepository.AddAsync(user, cancellationToken);

        if (request.DeptId != null && !string.IsNullOrEmpty(request.DeptName))
        {
            user.AssignDept(request.DeptId, request.DeptName);
        }

        if (request.SetAsDeptResponsibleUser || request.SetAsDefaultDeptResponsibleUser)
        {
            if (request.DeptId is null || request.DeptId == DeptId.Unassigned)
            {
                throw new KnownException("设为部门负责人时必须选择部门", ErrorCodes.DeptNotFound);
            }

            // 这里只表达用户创建时的跨聚合协作意图，实际负责人关系由部门命令加载部门聚合后写入。
            user.RequestDeptResponsibleUserAssignment(request.DeptId, request.SetAsDefaultDeptResponsibleUser);
        }

        if (request.PositionId != null && !string.IsNullOrEmpty(request.PositionName))
        {
            user.AssignPosition(request.PositionId, request.PositionName);
        }

        return user.Id;
    }
}

