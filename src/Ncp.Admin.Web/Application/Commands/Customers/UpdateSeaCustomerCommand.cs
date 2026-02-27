using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;
using Ncp.Admin.Domain;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Customers;

/// <summary>
/// 公海客户更新命令，区域名称、客户来源名称均由前端传入，应用层不再依赖区域/客户来源仓储解析。
/// </summary>
public record UpdateSeaCustomerCommand(
    CustomerId Id,
    CustomerSourceId CustomerSourceId,
    string CustomerSourceName,
    string FullName,
    string ShortName,
    string Nature,
    string ProvinceCode,
    string CityCode,
    string DistrictCode,
    string ProvinceName,
    string CityName,
    string DistrictName,
    string PhoneProvinceCode,
    string PhoneCityCode,
    string PhoneDistrictCode,
    string PhoneProvinceName,
    string PhoneCityName,
    string PhoneDistrictName,
    string ConsultationContent,
    string CoverRegion,
    string RegisterAddress,
    string MainContactName,
    string MainContactPhone,
    string ContactQq,
    string ContactWechat,
    string WechatStatus,
    string Remark,
    bool IsKeyAccount,
    IReadOnlyList<IndustryId> IndustryIds) : ICommand<bool>;

public class UpdateSeaCustomerCommandValidator : AbstractValidator<UpdateSeaCustomerCommand>
{
    public UpdateSeaCustomerCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.FullName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ShortName).MaximumLength(100);
    }
}

public class UpdateSeaCustomerCommandHandler(ICustomerRepository repository)
    : ICommandHandler<UpdateSeaCustomerCommand, bool>
{
    public async Task<bool> Handle(UpdateSeaCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到客户", ErrorCodes.CustomerNotFound);
        customer.UpdateWhenInSea(
            request.CustomerSourceId, request.CustomerSourceName , request.FullName,
            request.ShortName , request.Nature , request.ProvinceCode ,
            request.CityCode , request.DistrictCode ,
            request.ProvinceName , request.CityName , request.DistrictName ,
            request.PhoneProvinceCode , request.PhoneCityCode , request.PhoneDistrictCode ,
            request.PhoneProvinceName , request.PhoneCityName , request.PhoneDistrictName ,
            request.ConsultationContent ,
            request.CoverRegion , request.RegisterAddress , request.MainContactName , request.MainContactPhone ,
            request.ContactQq , request.ContactWechat ,
            request.WechatStatus , request.Remark , request.IsKeyAccount,
            request.IndustryIds);
        return true;
    }
}
