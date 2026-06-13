using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CAPLock",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Instance = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    LastLockTime = table.Column<DateTime>(type: "TIMESTAMP", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPLock", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "CAPPublishedMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Retries = table.Column<int>(type: "integer", nullable: true),
                    Added = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    StatusName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPPublishedMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CAPReceivedMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Name = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Group = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "TEXT", nullable: true),
                    Retries = table.Column<int>(type: "integer", nullable: true),
                    Added = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    StatusName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CAPReceivedMessage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dept",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "部门标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "部门名称"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "备注"),
                    ParentId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "状态（0=禁用，1=启用）"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序号"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间"),
                    RowVersion = table.Column<int>(type: "integer", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dept", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notification",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "通知标识"),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "标题"),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "内容"),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "类型"),
                    Level = table.Column<int>(type: "integer", nullable: false, comment: "等级"),
                    SenderId = table.Column<long>(type: "bigint", nullable: false, comment: "发送人用户ID"),
                    SenderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "发送人姓名"),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false, comment: "接收人用户ID"),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已读"),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "已读时间"),
                    BusinessId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "业务ID（字符串）"),
                    BusinessType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "业务类型"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "operation_log",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "操作日志ID"),
                    OperatorUserId = table.Column<long>(type: "bigint", nullable: false, comment: "操作人用户ID"),
                    OperatorUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "操作人姓名"),
                    Module = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "模块名称"),
                    OperationType = table.Column<int>(type: "integer", nullable: false, comment: "操作类型"),
                    RequestPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false, comment: "请求路径"),
                    RequestMethod = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, comment: "HTTP方法"),
                    HttpStatusCode = table.Column<int>(type: "integer", nullable: false, comment: "HTTP状态码"),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false, comment: "是否成功"),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "客户端IP"),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false, comment: "User-Agent"),
                    RequestBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "请求入参(JSON,脱敏/截断)"),
                    ResponseBody = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "响应出参(JSON,脱敏/截断)"),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false, comment: "请求耗时(毫秒)"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "操作时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "permission_preset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "权限预设包标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "预设名称"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "说明"),
                    PermissionCodesJson = table.Column<string>(type: "text", nullable: false, comment: "权限码 JSON 数组"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, comment: "是否启用"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, comment: "是否为系统默认配置包"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission_preset", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "position",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "岗位标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "岗位名称"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "岗位编码"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "岗位描述"),
                    DeptId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序号"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "状态（0=禁用，1=启用）"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "角色标识"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "角色名称"),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "角色描述"),
                    DataScope = table.Column<int>(type: "integer", nullable: false, comment: "数据权限范围"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, comment: "是否启用"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RowVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "用户标识"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "用户名"),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "邮箱"),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "手机号"),
                    RealName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "真实姓名"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "密码哈希"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, comment: "是否启用"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false, comment: "创建人用户ID"),
                    ModifierId = table.Column<long>(type: "bigint", nullable: false, comment: "修改人用户ID"),
                    DeleterId = table.Column<long>(type: "bigint", nullable: false, comment: "删除人用户ID"),
                    LastLoginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "最后登录时间"),
                    LastLoginIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "最后登录IP"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    RowVersion = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已删除"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间"),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "性别"),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "出生日期"),
                    IdCardNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "身份证号"),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "地址"),
                    Education = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "学历"),
                    GraduateSchool = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "毕业院校"),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "头像地址"),
                    NotOrderMeal = table.Column<bool>(type: "boolean", nullable: false, comment: "是否不订餐"),
                    OrderMealSort = table.Column<int>(type: "integer", nullable: false, comment: "订餐排序"),
                    WechatGuid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "唯一码"),
                    IsResigned = table.Column<bool>(type: "boolean", nullable: false, comment: "是否离职"),
                    ResignedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "离职时间"),
                    AttendanceRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "是否需要参与考勤计算"),
                    AttendanceRuleCode = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "考勤规则业务编码")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_calendar_memo",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "便签标识"),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "用户标识"),
                    MemoDate = table.Column<DateOnly>(type: "date", nullable: false, comment: "便签日期"),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "便签内容"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_calendar_memo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_home_dashboard_preference",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "用户标识"),
                    CardOrderJson = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "首页卡片排序 JSON"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_home_dashboard_preference", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_definition",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "主键"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "流程名称"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "流程描述"),
                    Version = table.Column<int>(type: "integer", nullable: false, comment: "版本号"),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "流程分类"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "流程状态：0草稿 1已发布 2已归档"),
                    DesignerSchemaJson = table.Column<string>(type: "text", nullable: false, comment: "设计器 Schema JSON"),
                    BasedOnId = table.Column<Guid>(type: "uuid", nullable: false, comment: "基于哪条流程定义创建（新版本时指向源定义，发布时据此归档源）"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "创建人ID"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否删除"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_definition", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "workflow_instance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "主键"),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false, comment: "流程定义ID"),
                    WorkflowDefinitionVersionId = table.Column<Guid>(type: "uuid", nullable: false, comment: "流程定义版本ID"),
                    WorkflowDefinitionName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "流程定义名称"),
                    BusinessKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "业务关联键"),
                    BusinessType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "业务类型"),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "流程标题"),
                    InitiatorId = table.Column<long>(type: "bigint", nullable: false, comment: "发起人ID"),
                    InitiatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "发起人姓名"),
                    InitiatorDeptId = table.Column<long>(type: "bigint", nullable: false, comment: "发起人部门ID"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "流程状态"),
                    CurrentNodeKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "当前节点key"),
                    CurrentNodeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "当前节点名称"),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "开始时间"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "完成时间"),
                    SuspendedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "最近一次挂起时间"),
                    ResumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "最近一次恢复时间"),
                    Variables = table.Column<string>(type: "text", nullable: false, comment: "流程变量JSON"),
                    Remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "备注"),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "业务执行失败原因")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_instance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dept_responsible_user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "部门负责人关系标识"),
                    DeptId = table.Column<long>(type: "bigint", nullable: false, comment: "部门ID"),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "负责人用户ID"),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, comment: "是否默认负责人"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序号"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dept_responsible_user", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dept_responsible_user_dept_DeptId",
                        column: x => x.DeptId,
                        principalTable: "dept",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_data_dept",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "角色ID"),
                    DeptId = table.Column<long>(type: "bigint", nullable: false, comment: "部门ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_data_dept", x => new { x.RoleId, x.DeptId });
                    table.ForeignKey(
                        name: "FK_role_data_dept_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "角色ID"),
                    PermissionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "权限编码"),
                    PermissionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "权限名称"),
                    PermissionDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "权限描述")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permission", x => new { x.RoleId, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_role_permission_role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_dept",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    DeptId = table.Column<long>(type: "bigint", nullable: false),
                    DeptName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_dept", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_dept_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "user_position",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    PositionId = table.Column<long>(type: "bigint", nullable: false),
                    PositionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_position", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_user_position_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "user_refresh_token",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_refresh_token", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_refresh_token_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_role",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_role_user_UserId",
                        column: x => x.UserId,
                        principalTable: "user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_definition_version",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "主键"),
                    WorkflowDefinitionId = table.Column<Guid>(type: "uuid", nullable: false, comment: "流程定义ID"),
                    Version = table.Column<int>(type: "integer", nullable: false, comment: "版本号"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "版本状态：0草稿 1已发布 2已归档"),
                    DesignerSchemaJson = table.Column<string>(type: "text", nullable: false, comment: "前端设计器 JSON"),
                    GraphSnapshotJson = table.Column<string>(type: "text", nullable: false, comment: "发布后的运行图快照 JSON"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    PublishedBy = table.Column<long>(type: "bigint", nullable: false, comment: "发布人ID"),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "发布时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_definition_version", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_definition_version_workflow_definition_WorkflowDef~",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "workflow_definition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_task",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "主键"),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "流程实例ID"),
                    NodeKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "节点key"),
                    NodeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "节点名称"),
                    TaskType = table.Column<int>(type: "integer", nullable: false, comment: "任务类型：0审批 1通知 2抄送"),
                    AssigneeType = table.Column<int>(type: "integer", nullable: false, comment: "处理人类型：0用户 1角色"),
                    AssigneeId = table.Column<long>(type: "bigint", nullable: false, comment: "处理人用户ID（按角色任务时为 0）"),
                    AssigneeRoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "处理人角色ID（按用户任务时为 Guid.Empty）"),
                    AssigneeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "处理人姓名/角色名"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "任务状态"),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "审批意见"),
                    ExtraDataJson = table.Column<string>(type: "text", nullable: false, comment: "任务扩展数据JSON"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "完成时间"),
                    CompletedByUserId = table.Column<long>(type: "bigint", nullable: false, comment: "审批通过时的实际操作人用户ID（角色任务等用于追溯）"),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_task", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_task_workflow_instance_WorkflowInstanceId",
                        column: x => x.WorkflowInstanceId,
                        principalTable: "workflow_instance",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_task_assignment_snapshot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "主键"),
                    WorkflowTaskId = table.Column<Guid>(type: "uuid", nullable: false, comment: "任务ID"),
                    AssigneeType = table.Column<int>(type: "integer", nullable: false, comment: "处理人类型：0用户 1角色"),
                    AssigneeUserId = table.Column<long>(type: "bigint", nullable: false, comment: "处理人用户ID"),
                    AssigneeRoleId = table.Column<Guid>(type: "uuid", nullable: false, comment: "处理人角色ID"),
                    AssigneeDisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "处理人显示名"),
                    AssignmentSource = table.Column<int>(type: "integer", nullable: false, comment: "授权来源"),
                    SourceRuleId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "来源规则ID"),
                    VisibilityMode = table.Column<int>(type: "integer", nullable: false, comment: "可见性模式"),
                    BypassDataPermission = table.Column<bool>(type: "boolean", nullable: false, comment: "是否绕过常规数据权限过滤"),
                    InitiatorDeptScopeMode = table.Column<int>(type: "integer", nullable: false, comment: "发起部门范围模式"),
                    InitiatorDeptScopeDeptIdsJson = table.Column<string>(type: "text", nullable: false, comment: "配置的发起部门范围JSON"),
                    CreatedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "创建原因"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "创建时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_task_assignment_snapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_task_assignment_snapshot_workflow_task_WorkflowTas~",
                        column: x => x.WorkflowTaskId,
                        principalTable: "workflow_task",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt_StatusName",
                table: "CAPPublishedMessage",
                columns: new[] { "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_Version_ExpiresAt_StatusName",
                table: "CAPPublishedMessage",
                columns: new[] { "Version", "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_ExpiresAt_StatusName1",
                table: "CAPReceivedMessage",
                columns: new[] { "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_Version_ExpiresAt_StatusName1",
                table: "CAPReceivedMessage",
                columns: new[] { "Version", "ExpiresAt", "StatusName" });

            migrationBuilder.CreateIndex(
                name: "IX_dept_IsDeleted",
                table: "dept",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_dept_ParentId",
                table: "dept",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_dept_SortOrder",
                table: "dept",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_dept_Status",
                table: "dept",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_dept_responsible_user_DeptId",
                table: "dept_responsible_user",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_dept_responsible_user_DeptId_UserId",
                table: "dept_responsible_user",
                columns: new[] { "DeptId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dept_responsible_user_UserId",
                table: "dept_responsible_user",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_IsDeleted",
                table: "notification",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_notification_IsRead",
                table: "notification",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_notification_ReceiverId",
                table: "notification",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_ReceiverId_IsRead_IsDeleted",
                table: "notification",
                columns: new[] { "ReceiverId", "IsRead", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_Type",
                table: "notification",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_operation_log_CreatedAt",
                table: "operation_log",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_operation_log_Module",
                table: "operation_log",
                column: "Module");

            migrationBuilder.CreateIndex(
                name: "IX_operation_log_OperationType",
                table: "operation_log",
                column: "OperationType");

            migrationBuilder.CreateIndex(
                name: "IX_operation_log_OperatorUserId",
                table: "operation_log",
                column: "OperatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_permission_preset_Name",
                table: "permission_preset",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_permission_preset_SortOrder",
                table: "permission_preset",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_position_Code",
                table: "position",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_position_DeptId",
                table: "position",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_position_IsDeleted",
                table: "position",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_position_Status",
                table: "position",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_role_Name",
                table: "role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_user_Name",
                table: "user",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_user_calendar_memo_UserId_MemoDate",
                table: "user_calendar_memo",
                columns: new[] { "UserId", "MemoDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_dept_DeptId",
                table: "user_dept",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_user_dept_UserId",
                table: "user_dept",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_position_PositionId",
                table: "user_position",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_user_position_UserId",
                table: "user_position",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_refresh_token_UserId",
                table: "user_refresh_token",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_BasedOnId",
                table: "workflow_definition",
                column: "BasedOnId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_Category",
                table: "workflow_definition",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_IsDeleted",
                table: "workflow_definition",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_Name",
                table: "workflow_definition",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_Status",
                table: "workflow_definition",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_version_WorkflowDefinitionId",
                table: "workflow_definition_version",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_version_WorkflowDefinitionId_Status",
                table: "workflow_definition_version",
                columns: new[] { "WorkflowDefinitionId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definition_version_WorkflowDefinitionId_Version",
                table: "workflow_definition_version",
                columns: new[] { "WorkflowDefinitionId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflow_instance_active_business",
                table: "workflow_instance",
                columns: new[] { "BusinessType", "BusinessKey" },
                unique: true,
                filter: "\"Status\" IN (0, 1)");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_BusinessKey",
                table: "workflow_instance",
                column: "BusinessKey");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_BusinessType",
                table: "workflow_instance",
                column: "BusinessType");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_BusinessType_BusinessKey_Status",
                table: "workflow_instance",
                columns: new[] { "BusinessType", "BusinessKey", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_InitiatorDeptId",
                table: "workflow_instance",
                column: "InitiatorDeptId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_InitiatorId",
                table: "workflow_instance",
                column: "InitiatorId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_Status",
                table: "workflow_instance",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_WorkflowDefinitionId",
                table: "workflow_instance",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_instance_WorkflowDefinitionVersionId",
                table: "workflow_instance",
                column: "WorkflowDefinitionVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_AssigneeId",
                table: "workflow_task",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_AssigneeId_Status",
                table: "workflow_task",
                columns: new[] { "AssigneeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_AssigneeRoleId",
                table: "workflow_task",
                column: "AssigneeRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_Status",
                table: "workflow_task",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_WorkflowInstanceId",
                table: "workflow_task",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_assignment_snapshot_AssigneeRoleId",
                table: "workflow_task_assignment_snapshot",
                column: "AssigneeRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_assignment_snapshot_AssigneeRoleId_WorkflowTa~",
                table: "workflow_task_assignment_snapshot",
                columns: new[] { "AssigneeRoleId", "WorkflowTaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_assignment_snapshot_AssigneeUserId",
                table: "workflow_task_assignment_snapshot",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_assignment_snapshot_AssigneeUserId_WorkflowTa~",
                table: "workflow_task_assignment_snapshot",
                columns: new[] { "AssigneeUserId", "WorkflowTaskId" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_task_assignment_snapshot_WorkflowTaskId",
                table: "workflow_task_assignment_snapshot",
                column: "WorkflowTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CAPLock");

            migrationBuilder.DropTable(
                name: "CAPPublishedMessage");

            migrationBuilder.DropTable(
                name: "CAPReceivedMessage");

            migrationBuilder.DropTable(
                name: "dept_responsible_user");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "operation_log");

            migrationBuilder.DropTable(
                name: "permission_preset");

            migrationBuilder.DropTable(
                name: "position");

            migrationBuilder.DropTable(
                name: "role_data_dept");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "user_calendar_memo");

            migrationBuilder.DropTable(
                name: "user_dept");

            migrationBuilder.DropTable(
                name: "user_home_dashboard_preference");

            migrationBuilder.DropTable(
                name: "user_position");

            migrationBuilder.DropTable(
                name: "user_refresh_token");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "workflow_definition_version");

            migrationBuilder.DropTable(
                name: "workflow_task_assignment_snapshot");

            migrationBuilder.DropTable(
                name: "dept");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "workflow_definition");

            migrationBuilder.DropTable(
                name: "workflow_task");

            migrationBuilder.DropTable(
                name: "workflow_instance");
        }
    }
}
