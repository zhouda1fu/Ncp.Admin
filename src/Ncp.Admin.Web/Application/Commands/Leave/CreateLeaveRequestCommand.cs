using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Leave;

/// <summary>
/// 创建请假申请（草稿）
/// </summary>
public record CreateLeaveRequestCommand(
    UserId ApplicantId,
    string ApplicantName,
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal Days,
    string Reason) : ICommand<LeaveRequestId>;

public class CreateLeaveRequestCommandValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestCommandValidator()
    {
        RuleFor(c => c.ApplicantId).NotNull().WithMessage("申请人不能为空");
        RuleFor(c => c.ApplicantName).NotEmpty().WithMessage("申请人姓名不能为空").MaximumLength(100);
        RuleFor(c => c.LeaveType).IsInEnum().WithMessage("无效的请假类型");
        RuleFor(c => c.StartDate).NotEmpty().WithMessage("开始日期不能为空");
        RuleFor(c => c.EndDate).GreaterThanOrEqualTo(c => c.StartDate).WithMessage("结束日期不能早于开始日期");
        RuleFor(c => c.Days).GreaterThan(0).WithMessage("请假天数必须大于0");
        RuleFor(c => c.Reason).MaximumLength(500);
    }
}

public class CreateLeaveRequestCommandHandler(ILeaveRequestRepository repository)
    : ICommandHandler<CreateLeaveRequestCommand, LeaveRequestId>
{
    public async Task<LeaveRequestId> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var entity = new LeaveRequest(
            request.ApplicantId,
            request.ApplicantName,
            request.LeaveType,
            request.StartDate,
            request.EndDate,
            request.Days,
            request.Reason ?? string.Empty);
        await repository.AddAsync(entity, cancellationToken);
        return entity.Id;
    }
}
