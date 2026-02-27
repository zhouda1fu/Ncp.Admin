---
applyTo: "src/Ncp.Admin.Web/Endpoints/**/*.cs"
---

# Endpoint 开发指南

## 开发原则

### 必须

- **端点定义**：
    - 继承对应的 `Endpoint` 基类。
    - 必须为每个 Endpoint 单独定义请求 DTO 和响应 DTO。
    - 请求 DTO、响应 DTO 与端点定义在同一文件中。
    - 不同的 Endpoint 放在不同文件中。
    - 使用 `ResponseData<T>` 包装响应数据。
    - 使用主构造函数注入依赖的服务，如 `IMediator`。
- **配置与实现**：
    - 使用特性方式配置路由和权限：`[HttpPost("/api/...")]`、`[AllowAnonymous]` 等。
    - 在 `HandleAsync()` 方法中处理业务逻辑。
    - 使用构造函数注入 `IMediator` 发送命令或查询。
    - 使用 `Send.OkAsync()`、`Send.CreatedAsync()`、`Send.NoContentAsync()` 发送响应。
    - 使用 `.AsResponseData()` 扩展方法创建响应数据。
- **Request/Response 与文档注释**（与 `.cursor/skills/ncp-admin-ddd/SKILL.md` 中「API 端点 Request/Response 格式规范」一致）：
    - Request/Response **必须使用 record**，优先使用位置参数形式。
    - 所有 ID 使用强类型：`CustomerId`、`UserId`、`DeptId`、`CustomerSourceId`、`IndustryId`、`CustomerContactId` 等，禁止裸 `Guid`/`long`/`string`。
    - 每个 record 上方有 `<summary>`，每个参数有 `<param name="参数名">描述</param>`。
    - 端点类有 `<summary>`，主构造函数依赖有 `<param name="...">`。
    - 参考：`Endpoints/Identity/Admin/UserEndpoints/UpdateUserEndpoint.cs`、`Endpoints/Customer/*.cs`。
- **强类型ID处理**：
    - 在 DTO 中直接使用强类型 ID 类型，如 `UserId`、`CustomerId`、`DeptId`。
    - 依赖框架的隐式转换处理类型转换。
- **引用**：
    - 引用 `FastEndpoints` 和 `Microsoft.AspNetCore.Authorization`。

### 必须不要

- **配置方式**：使用属性特性而不是 `Configure()` 方法来配置端点。
- **强类型ID**：避免使用 `.Value` 属性访问内部值。

## 文件命名规则

- 类文件应放置在 `src/Ncp.Admin.Web/Endpoints/{Module}/` 目录下。
- 端点文件名格式为 `{Action}{Entity}Endpoint.cs`。
- 请求 DTO、响应 DTO 与端点定义在同一文件中。

## 代码示例

**参考实现**：`Endpoints/Identity/Admin/UserEndpoints/UpdateUserEndpoint.cs`、`Endpoints/Customer/UpdateCustomerEndpoint.cs`。

Request/Response 使用 record + 强类型 ID + 完整文档注释示例：

```csharp
/// <summary>
/// 更新客户请求
/// </summary>
/// <param name="Id">客户 ID</param>
/// <param name="OwnerId">负责人用户 ID，null 表示公海</param>
/// <param name="DeptId">所属部门 ID</param>
/// <param name="CustomerSourceId">客户来源 ID</param>
/// <param name="FullName">客户全称</param>
public record UpdateCustomerRequest(
    CustomerId Id,
    UserId? OwnerId,
    DeptId? DeptId,
    CustomerSourceId CustomerSourceId,
    string FullName
    /* 其余参数... */);

/// <summary>
/// 更新客户
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UpdateCustomerEndpoint(IMediator mediator) : Endpoint<UpdateCustomerRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Put("/api/admin/customers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateCustomerRequest req, CancellationToken ct)
    {
        var cmd = new UpdateCustomerCommand(req.Id, req.OwnerId, req.DeptId, req.CustomerSourceId, req.FullName, ...);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
```

### 更多端点响应示例

#### 创建资源的端点
```csharp
public override async Task HandleAsync(CreateUserRequestDto req, CancellationToken ct)
{
    var command = new CreateUserCommand(req.Name, req.Email);
    var userId = await mediator.Send(command, ct);
    
    // ...existing code...
    var response = new CreateUserResponseDto(userId);
    await Send.CreatedAsync(response.AsResponseData(), ct);
}
```

#### 查询资源的端点  
```csharp
public override async Task HandleAsync(GetUserRequestDto req, CancellationToken ct)
{
    var query = new GetUserQuery(req.UserId);
    var user = await mediator.Send(query, ct);
    
    await Send.OkAsync(user.AsResponseData(), ct);
}
```

#### 更新资源的端点
```csharp
public override async Task HandleAsync(UpdateUserRequestDto req, CancellationToken ct)
{
    var command = new UpdateUserCommand(req.UserId, req.Name, req.Email);
    await mediator.Send(command, ct);
    
    await Send.NoContentAsync(ct);
}
```

## 配置方式

端点使用属性模式配置，不使用 `Configure()` 方法：

```csharp
[Tags("ModuleName")]
[HttpPost("/api/resource")]
[AllowAnonymous]
public class CreateResourceEndpoint(IMediator mediator) : Endpoint<CreateRequest, ResponseData<CreateResponse>>
{
    // 实现
}
```
