using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Projects;

/// <summary>
/// 创建项目命令（冗余名称由前端传入，不通过加载主数据解析）
/// </summary>
public record CreateProjectCommand(
    UserId CreatorId,
    string CreatorName,
    CustomerId CustomerId,
    string CustomerName,
    ProjectTypeId ProjectTypeId,
    string ProjectTypeName,
    ProjectStatusOptionId ProjectStatusOptionId,
    string ProjectStatusOptionName,
    ProjectIndustryId ProjectIndustryId,
    string ProjectIndustryName,
    RegionId ProvinceRegionId,
    string ProvinceName,
    RegionId CityRegionId,
    string CityName,
    RegionId DistrictRegionId,
    string DistrictName,
    string Name,
    string ProjectNumber = "",
    DateOnly? StartDate = null,
    decimal Budget = 0,
    decimal PurchaseAmount = 0,
    string ProjectContent = "") : ICommand<ProjectId>;

/// <summary>
/// 创建项目命令验证器
/// </summary>
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(c => c.CreatorId).NotNull();
        RuleFor(c => c.CreatorName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.CustomerId).NotNull();
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ProjectTypeId).NotNull();
        RuleFor(c => c.ProjectTypeName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ProjectStatusOptionId).NotNull();
        RuleFor(c => c.ProjectStatusOptionName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ProjectIndustryName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ProvinceName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.CityName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.DistrictName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.ProjectIndustryId).NotNull();
        RuleFor(c => c.ProvinceRegionId).NotNull();
        RuleFor(c => c.CityRegionId).NotNull();
        RuleFor(c => c.DistrictRegionId).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ProjectNumber).NotEmpty().MaximumLength(50);
    }
}

/// <summary>
/// 创建项目命令处理器（仅校验客户存在且未作废，冗余名称使用前端传入值）
/// </summary>
public class CreateProjectCommandHandler(
    IProjectRepository repository,
    ICustomerRepository customerRepository) : ICommandHandler<CreateProjectCommand, ProjectId>
{
    public async Task<ProjectId> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetAsync(request.CustomerId, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        if (customer.IsVoided)
            throw new KnownException("客户已作废，无法创建项目", ErrorCodes.CustomerNotFound);

        var project = new Project(
            request.CustomerId,
            request.CustomerName ?? string.Empty,
            request.ProjectTypeId,
            request.ProjectTypeName ?? string.Empty,
            request.ProjectStatusOptionId,
            request.ProjectStatusOptionName ?? string.Empty,
            request.ProjectIndustryId,
            request.ProjectIndustryName ?? string.Empty,
            request.ProvinceRegionId,
            request.ProvinceName ?? string.Empty,
            request.CityRegionId,
            request.CityName ?? string.Empty,
            request.DistrictRegionId,
            request.DistrictName ?? string.Empty,
            request.Name,
            request.CreatorId,
            request.CreatorName ?? string.Empty,
            request.ProjectNumber ?? string.Empty,
            request.StartDate,
            request.Budget,
            request.PurchaseAmount,
            request.ProjectContent ?? string.Empty);
        await repository.AddAsync(project, cancellationToken);
        return project.Id;
    }
}
