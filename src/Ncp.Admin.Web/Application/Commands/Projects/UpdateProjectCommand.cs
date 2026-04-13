using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Projects;

/// <summary>
/// 更新项目命令（冗余名称由前端传入，不通过加载主数据解析）
/// </summary>
public record UpdateProjectCommand(
    ProjectId Id,
    string Name,
    ProjectTypeId ProjectTypeId,
    string ProjectTypeName,
    ProjectStatusOptionId ProjectStatusOptionId,
    string ProjectStatusOptionName,
    string ProjectNumber,
    ProjectIndustryId ProjectIndustryId,
    string ProjectIndustryName,
    RegionId ProvinceRegionId,
    string ProvinceName,
    RegionId CityRegionId,
    string CityName,
    RegionId DistrictRegionId,
    string DistrictName,
    DateOnly? StartDate,
    decimal Budget,
    decimal PurchaseAmount,
    string ProjectContent) : ICommand<bool>;

/// <summary>
/// 更新项目命令验证器
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(c => c.Id).NotNull();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ProjectTypeId).NotNull();
        RuleFor(c => c.ProjectTypeName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ProjectStatusOptionId).NotNull();
        RuleFor(c => c.ProjectStatusOptionName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ProjectIndustryId).NotNull();
        RuleFor(c => c.ProjectIndustryName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ProvinceRegionId).NotNull();
        RuleFor(c => c.ProvinceName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.CityRegionId).NotNull();
        RuleFor(c => c.CityName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.DistrictRegionId).NotNull();
        RuleFor(c => c.DistrictName).NotEmpty().MaximumLength(50);
    }
}

/// <summary>
/// 更新项目命令处理器（冗余名称使用前端传入值）
/// </summary>
public class UpdateProjectCommandHandler(IProjectRepository repository) : ICommandHandler<UpdateProjectCommand, bool>
{
    public async Task<bool> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到项目", ErrorCodes.ProjectNotFound);
        project.Update(
            request.Name,
            request.ProjectTypeId,
            request.ProjectTypeName ?? string.Empty,
            request.ProjectStatusOptionId,
            request.ProjectStatusOptionName ?? string.Empty,
            request.ProjectNumber ?? string.Empty,
            request.ProjectIndustryId,
            request.ProjectIndustryName ?? string.Empty,
            request.ProvinceRegionId,
            request.ProvinceName ?? string.Empty,
            request.CityRegionId,
            request.CityName ?? string.Empty,
            request.DistrictRegionId,
            request.DistrictName ?? string.Empty,
            request.StartDate,
            request.Budget,
            request.PurchaseAmount,
            request.ProjectContent ?? string.Empty);
        return true;
    }
}
