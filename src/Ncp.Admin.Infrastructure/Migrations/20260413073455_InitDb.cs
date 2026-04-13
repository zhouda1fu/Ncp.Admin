using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ncp.Admin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "announcement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "公告标识"),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "公告标题"),
                    Content = table.Column<string>(type: "text", nullable: false, comment: "公告正文"),
                    PublisherId = table.Column<long>(type: "bigint", nullable: false, comment: "发布人用户ID"),
                    PublisherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "发布人姓名"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "公告状态"),
                    PublishAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "发布时间（草稿为 null）"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "announcement_read_record",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "已读记录标识"),
                    AnnouncementId = table.Column<Guid>(type: "uuid", nullable: false, comment: "公告ID"),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "用户ID"),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "已读时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_announcement_read_record", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "asset",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "资产标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "分类"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "编码"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "状态"),
                    PurchaseDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "购置日期"),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "价值"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "备注"),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false, comment: "创建人用户ID"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "asset_allocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssetId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    AllocatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReturnedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_allocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "attendance_record",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CheckInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CheckOutAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_record", x => x.Id);
                });

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
                name: "chat_group",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chat_message",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    ReplyToMessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_message", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contact_group",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contract",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "合同标识"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "合同编号"),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "合同标题"),
                    PartyA = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "甲方"),
                    PartyB = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "乙方"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "合同金额"),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "开始日期"),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "结束日期（到期日）"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "合同状态"),
                    FileStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "附件存储 Key"),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false, comment: "创建人用户ID"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "关联订单ID"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "关联客户ID"),
                    ContractType = table.Column<int>(type: "integer", nullable: false, comment: "合同类型（TypeValue）"),
                    ContractTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "合同类型名称"),
                    IncomeExpenseType = table.Column<int>(type: "integer", nullable: false, comment: "收支类型（TypeValue）"),
                    IncomeExpenseTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "收支类型名称"),
                    SignDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "签约日期"),
                    DepartmentId = table.Column<long>(type: "bigint", nullable: false, comment: "部门ID"),
                    BusinessManager = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "业务经理"),
                    ResponsibleProject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "负责项目"),
                    InputCustomer = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "录入客户（名称或标识）"),
                    NextPaymentReminder = table.Column<bool>(type: "boolean", nullable: false, comment: "下次收付款报警"),
                    ContractExpiryReminder = table.Column<bool>(type: "boolean", nullable: false, comment: "合同过期报警"),
                    SingleDoubleSeal = table.Column<int>(type: "integer", nullable: false, comment: "单双章（0=单章 1=双章）"),
                    InvoicingInformation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "开票信息"),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false, comment: "到款情况（TypeValue）"),
                    WarrantyPeriod = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "质保期"),
                    IsInstallmentPayment = table.Column<bool>(type: "boolean", nullable: false, comment: "是否分期"),
                    AccumulatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "累计金额"),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "备注"),
                    Description = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false, comment: "合同内容/描述"),
                    ApprovedBy = table.Column<long>(type: "bigint", nullable: false, comment: "审批人"),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "审批时间"),
                    RowVersion = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "contract_type_option",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "合同类型选项标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    TypeValue = table.Column<int>(type: "integer", nullable: false, comment: "类型值"),
                    OrderSigningCompanyOptionDisplay = table.Column<bool>(type: "boolean", nullable: false, comment: "订单签订公司选项展示"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_type_option", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户标识"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false, comment: "负责人用户ID（公海为 0）"),
                    OwnerDeptId = table.Column<long>(type: "bigint", nullable: false, comment: "负责人部门ID（冗余，无部门为 0）"),
                    OwnerDeptName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "负责人部门名称（冗余）"),
                    CustomerSourceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户来源ID"),
                    CustomerSourceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "客户来源名称"),
                    IsVoided = table.Column<bool>(type: "boolean", nullable: false, comment: "是否作废"),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "客户全称"),
                    ShortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "客户简称"),
                    Status = table.Column<int>(type: "integer", nullable: true, comment: "客户状态"),
                    Nature = table.Column<int>(type: "integer", nullable: true, comment: "公司性质"),
                    ProvinceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "省区域码"),
                    CityCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "市区域码"),
                    DistrictCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "区/县区域码"),
                    ProvinceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "省名称"),
                    CityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "市名称"),
                    DistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "区/县名称"),
                    PhoneProvinceCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "电话省区域码"),
                    PhoneCityCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "电话市区域码"),
                    PhoneDistrictCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "电话区/县区域码"),
                    PhoneProvinceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "电话省名称"),
                    PhoneCityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "电话市名称"),
                    PhoneDistrictName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "电话区/县名称"),
                    ConsultationContent = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "咨询内容"),
                    CoverRegion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true, comment: "覆盖区域"),
                    RegisterAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "注册地址"),
                    EmployeeCount = table.Column<int>(type: "integer", nullable: false, comment: "员工数量"),
                    BusinessLicense = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "营业执照（路径或 URL）"),
                    MainContactName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "主联系人姓名"),
                    MainContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "主联系人电话"),
                    WechatStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "微信添加情况"),
                    Remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true, comment: "备注"),
                    ContactQq = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "QQ"),
                    ContactWechat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "微信"),
                    IsKeyAccount = table.Column<bool>(type: "boolean", nullable: false, comment: "是否重点客户"),
                    IsHidden = table.Column<bool>(type: "boolean", nullable: false, comment: "是否隐藏"),
                    CombineFlag = table.Column<bool>(type: "boolean", nullable: false, comment: "合并标记"),
                    IsInSea = table.Column<bool>(type: "boolean", nullable: false, comment: "是否在公海"),
                    ReleasedToSeaAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "释放到公海时间"),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false, comment: "创建人用户ID"),
                    CreatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "创建人姓名"),
                    OwnerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "负责人姓名（冗余）"),
                    ClaimedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "领用时间"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customer_sea_region_assignment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户公海片区分配ID"),
                    TargetUserId = table.Column<long>(type: "bigint", nullable: false, comment: "目标用户ID"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间（UTC）"),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_sea_region_assignment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customer_sea_region_assignment_audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "公海片区分配审计ID"),
                    TargetUserId = table.Column<long>(type: "bigint", nullable: false, comment: "被修改用户ID"),
                    OperatorUserId = table.Column<long>(type: "bigint", nullable: false, comment: "操作人用户ID"),
                    OperatorUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "操作人姓名"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "操作时间（UTC）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_sea_region_assignment_audit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customer_source",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户来源标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序"),
                    UsageScene = table.Column<int>(type: "integer", nullable: false, comment: "使用场景：0公海 1客户列表 2通用")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_source", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "dept",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "部门标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "部门名称"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "备注"),
                    ParentId = table.Column<long>(type: "bigint", nullable: false),
                    ManagerId = table.Column<long>(type: "bigint", nullable: false, comment: "部门主管用户ID"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "状态（0=禁用，1=启用）"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
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
                name: "document",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "文档标识"),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "标题"),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false, comment: "创建人用户ID"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "expense_claim",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicantName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "工作流实例ID（未关联为 Guid.Empty）"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_claim", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "income_expense_type_option",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "收支类型选项标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    TypeValue = table.Column<int>(type: "integer", nullable: false, comment: "类型值"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_expense_type_option", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "industry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "行业标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false, comment: "父级行业ID（Guid.Empty 表示一级）"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "备注")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_industry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_balance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    LeaveType = table.Column<int>(type: "integer", nullable: false),
                    TotalDays = table.Column<decimal>(type: "numeric", nullable: false),
                    UsedDays = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_balance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_request",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<long>(type: "bigint", nullable: false),
                    ApplicantName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LeaveType = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Days = table.Column<decimal>(type: "numeric", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "工作流实例ID（未提交为 Guid.Empty）"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_request", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meeting_booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MeetingRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_booking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "meeting_room",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: false),
                    Equipment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_meeting_room", x => x.Id);
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
                    SenderId = table.Column<long>(type: "bigint", nullable: true, comment: "发送人用户ID"),
                    SenderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "发送人姓名"),
                    ReceiverId = table.Column<long>(type: "bigint", nullable: false, comment: "接收人用户ID"),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已读"),
                    ReadAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "已读时间"),
                    BusinessId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "业务ID（字符串）"),
                    BusinessType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "业务类型"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "操作时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "订单标识"),
                    Process = table.Column<int>(type: "integer", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "订单编号"),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "订单类型"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "订单状态"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户ID"),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "客户名称"),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false, comment: "项目ID"),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, comment: "金额"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false),
                    OwnerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DeptId = table.Column<long>(type: "bigint", nullable: false),
                    DeptName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProjectContactName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProjectContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Warranty = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ContractSigningCompany = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContractTrustee = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    NeedInvoice = table.Column<bool>(type: "boolean", nullable: false),
                    InvoiceTypeId = table.Column<long>(type: "bigint", nullable: false, comment: "发票类型ID"),
                    InstallationFee = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    EstimatedFreight = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ContractFilesJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    StockFilesJson = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    SelectedContractFileId = table.Column<int>(type: "integer", nullable: false, comment: "选择合同"),
                    IsShipped = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false, comment: "到款状态"),
                    ContractNotCompanyTemplate = table.Column<bool>(type: "boolean", nullable: false),
                    ContractAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceiverName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReceiverPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceiverAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PayDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeliveryDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OrderLogisticsCompanyId = table.Column<Guid>(type: "uuid", nullable: false, comment: "物流公司ID"),
                    OrderLogisticsMethodId = table.Column<Guid>(type: "uuid", nullable: false, comment: "物流方式ID"),
                    LogisticsPaymentMethodId = table.Column<int>(type: "integer", nullable: false, comment: "物流费用支付方式"),
                    WaybillNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "运单编号"),
                    ShippingFee = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, comment: "运费"),
                    ShippingFeeIsPay = table.Column<bool>(type: "boolean", nullable: false, comment: "是否付运费"),
                    Surcharge = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, comment: "附加费"),
                    IsNoLogo = table.Column<bool>(type: "boolean", nullable: false),
                    AfterSalesServiceId = table.Column<string>(type: "text", nullable: false),
                    IsAssess = table.Column<bool>(type: "boolean", nullable: false),
                    Comments = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsRed = table.Column<bool>(type: "boolean", nullable: false),
                    IsFree = table.Column<bool>(type: "boolean", nullable: false),
                    IsRepay = table.Column<bool>(type: "boolean", nullable: false),
                    RepayDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FRepayDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DelayDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DelayReason = table.Column<string>(type: "text", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    Scontent = table.Column<string>(type: "text", nullable: false),
                    WarehousePickerId = table.Column<long>(type: "bigint", nullable: false, comment: "配货人用户ID"),
                    WarehouseTechId = table.Column<long>(type: "bigint", nullable: false, comment: "仓库技术用户ID"),
                    WarehouseReviewerId = table.Column<long>(type: "bigint", nullable: false, comment: "复核人用户ID"),
                    WarehouseStatus = table.Column<int>(type: "integer", nullable: false, comment: "仓库状态"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间"),
                    RowVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    WorkflowInstanceId = table.Column<Guid>(type: "uuid", nullable: false, comment: "关联工作流实例ID（未关联为 Guid.Empty）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order_invoice_type_option",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "订单发票类型选项标识")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    TypeValue = table.Column<int>(type: "integer", nullable: false, comment: "类型值"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_invoice_type_option", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order_logistics_company",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeValue = table.Column<int>(type: "integer", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_logistics_company", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "order_logistics_method",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TypeValue = table.Column<int>(type: "integer", nullable: false),
                    Sort = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_logistics_method", x => x.Id);
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否软删"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_position", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "产品标识"),
                    ProductTypeId = table.Column<Guid>(type: "uuid", nullable: false, comment: "产品类型ID"),
                    Status = table.Column<bool>(type: "boolean", nullable: false, comment: "状态（是否有效）"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "产品名称"),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "产品编码"),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "型号"),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "单位"),
                    Barcode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "条码"),
                    ActivationCode = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "激活码"),
                    PriceStandard = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "价格标准"),
                    MarketSales = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "市场销售"),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "描述"),
                    CostPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, comment: "成本价"),
                    CustomerPrice = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, comment: "客户价"),
                    Qty = table.Column<int>(type: "integer", nullable: false, comment: "库存数量"),
                    Tags = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "标签"),
                    Feature = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "功能特点"),
                    Configuration = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "硬件配置"),
                    Instructions = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "使用说明"),
                    InstallProcess = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "操作流程"),
                    OperationProcessResources = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "操作流程资源JSON"),
                    Introduction = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "产品介绍"),
                    IntroductionResources = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "产品介绍资源JSON"),
                    ImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "图片路径"),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false, comment: "产品分类ID（无分类为 Guid.Empty）"),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false, comment: "供应商ID（无供应商为 Guid.Empty）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "产品分类标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "分类名称"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "备注"),
                    ParentId = table.Column<Guid>(type: "uuid", nullable: false, comment: "上级分类ID（Guid.Empty 表示根节点）"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序"),
                    Visible = table.Column<bool>(type: "boolean", nullable: false, comment: "是否可见"),
                    IsDiscount = table.Column<bool>(type: "boolean", nullable: false, comment: "是否优惠")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_parameter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "产品参数标识"),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false, comment: "所属产品ID"),
                    Year = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "参数年份"),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false, comment: "参数内容")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_parameter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "product_type",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "产品类型标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "类型名称"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序"),
                    Visible = table.Column<bool>(type: "boolean", nullable: false, comment: "是否启用")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "项目标识"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "项目名称"),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false, comment: "创建人用户ID"),
                    CreatorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "创建人姓名"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "项目状态"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProjectTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProjectStatusOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectStatusOptionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProjectNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProjectIndustryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectIndustryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProvinceRegionId = table.Column<long>(type: "bigint", nullable: false),
                    ProvinceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CityRegionId = table.Column<long>(type: "bigint", nullable: false),
                    CityName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DistrictRegionId = table.Column<long>(type: "bigint", nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Budget = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, comment: "项目预算"),
                    PurchaseAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProjectContent = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project_industry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "项目行业标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_industry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project_status_option",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "项目状态选项标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "编码"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_status_option", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project_task",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "任务标识"),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false, comment: "项目ID"),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "任务标题"),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "任务描述"),
                    AssigneeId = table.Column<long>(type: "bigint", nullable: false, comment: "负责人用户ID（0 表示未指定）"),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: true, comment: "截止日期"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "任务状态"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_task", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project_type",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "项目类型标识"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "名称"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_type", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "region",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "区域标识"),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "区域名称"),
                    ParentId = table.Column<long>(type: "bigint", nullable: false, comment: "父级区域ID"),
                    Level = table.Column<int>(type: "integer", nullable: false, comment: "层级"),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, comment: "排序")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_region", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "角色标识"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "角色名称"),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "角色描述"),
                    DataScope = table.Column<int>(type: "integer", nullable: false, comment: "数据权限范围"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
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
                name: "schedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ShiftName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedule", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "share_link",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_share_link", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "supplier",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "供应商标识"),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "供应商全称"),
                    ShortName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "简称"),
                    Contact = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "联系人"),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "电话"),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "邮箱"),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "地址"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "备注")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false, comment: "用户标识"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "用户名"),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "邮箱"),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "手机号"),
                    RealName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "真实姓名"),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "密码哈希"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, comment: "是否启用"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false, comment: "创建人用户ID"),
                    ModifierId = table.Column<long>(type: "bigint", nullable: false, comment: "修改人用户ID"),
                    DeleterId = table.Column<long>(type: "bigint", nullable: false, comment: "删除人用户ID"),
                    LastLoginTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "最后登录时间"),
                    LastLoginIp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "最后登录IP"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    RowVersion = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "是否已删除"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间"),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "性别"),
                    Age = table.Column<int>(type: "integer", nullable: false, comment: "年龄"),
                    BirthDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "出生日期"),
                    IdCardNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "身份证号"),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "地址"),
                    Education = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "学历"),
                    GraduateSchool = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "毕业院校"),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "头像地址"),
                    NotOrderMeal = table.Column<bool>(type: "boolean", nullable: false, comment: "是否不订餐"),
                    OrderMealSort = table.Column<int>(type: "integer", nullable: false, comment: "订餐排序"),
                    WechatGuid = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "唯一码"),
                    IsResigned = table.Column<bool>(type: "boolean", nullable: false, comment: "是否离职"),
                    ResignedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "离职时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "车辆标识"),
                    PlateNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, comment: "车牌号"),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "型号"),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "状态"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true, comment: "备注"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "更新时间"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookerId = table.Column<long>(type: "bigint", nullable: false),
                    Purpose = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_booking", x => x.Id);
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
                    DefinitionJson = table.Column<string>(type: "text", nullable: false, comment: "流程定义JSON（设计器树形结构）"),
                    BasedOnId = table.Column<Guid>(type: "uuid", nullable: false, comment: "基于哪条流程定义创建（新版本时指向源定义，发布时据此归档源）"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false, comment: "创建人ID"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
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
                    StartedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "开始时间"),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "完成时间"),
                    Variables = table.Column<string>(type: "text", nullable: false, comment: "流程变量JSON"),
                    Remark = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "备注"),
                    FailureReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true, comment: "业务执行失败原因")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_instance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "chat_group_member",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChatGroupId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chat_group_member", x => x.Id);
                    table.ForeignKey(
                        name: "FK_chat_group_member_chat_group_ChatGroupId",
                        column: x => x.ChatGroupId,
                        principalTable: "chat_group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contract_invoice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TaxRate = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    AmountExclTax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Source = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<bool>(type: "boolean", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    InvoicedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Handler = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BillingDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Remarks = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AttachmentStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contract_invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contract_invoice_contract_ContractId",
                        column: x => x.ContractId,
                        principalTable: "contract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "联系人标识"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户ID"),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, comment: "联系人姓名"),
                    ContactType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true, comment: "联系类型"),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    Birthday = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "职位"),
                    Mobile = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, comment: "手机"),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, comment: "电话"),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "邮箱"),
                    Qq = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "QQ"),
                    Wechat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "微信"),
                    IsWechatAdded = table.Column<bool>(type: "boolean", nullable: false, comment: "微信是否已添加"),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, comment: "是否主联系人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_contact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_contact_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_contact_record",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordType = table.Column<int>(type: "integer", nullable: false, comment: "联系类型：1电话 2出差 3微信 4其他"),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NextVisitAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, comment: "0待选择 1有效联系 2无效联系"),
                    OwnerId = table.Column<long>(type: "bigint", nullable: false, comment: "负责人用户ID"),
                    OwnerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OwnerDeptId = table.Column<long>(type: "bigint", nullable: false, comment: "负责人部门ID"),
                    OwnerDeptName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifierId = table.Column<long>(type: "bigint", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Remark = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReminderIntervalDays = table.Column<int>(type: "integer", nullable: false, comment: "提醒间隔天：1,2,3,10,15,20,30,50,80,100"),
                    ReminderCount = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CustomerAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    VisitAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, comment: "软删"),
                    DeletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "删除时间"),
                    DeleterId = table.Column<long>(type: "bigint", nullable: false, comment: "删除人")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_contact_record", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_contact_record_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_industry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户行业关联标识"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户ID"),
                    IndustryId = table.Column<Guid>(type: "uuid", nullable: false, comment: "行业ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_industry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_industry_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_sea_visibility_board",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户 ID（与客户主键一致）")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_sea_visibility_board", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_sea_visibility_board_customer_Id",
                        column: x => x.Id,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_share",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户ID"),
                    SharedToUserId = table.Column<long>(type: "bigint", nullable: false, comment: "共享给用户ID"),
                    SharedByUserId = table.Column<long>(type: "bigint", nullable: false, comment: "共享人用户ID"),
                    SharedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "共享时间")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_share", x => new { x.CustomerId, x.SharedToUserId });
                    table.ForeignKey(
                        name: "FK_customer_share_customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_sea_region_assignment_region",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_sea_region_assignment_region", x => new { x.AssignmentId, x.RegionId });
                    table.ForeignKey(
                        name: "FK_customer_sea_region_assignment_region_customer_sea_region_a~",
                        column: x => x.AssignmentId,
                        principalTable: "customer_sea_region_assignment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_sea_region_assignment_audit_detail",
                columns: table => new
                {
                    AuditId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<long>(type: "bigint", nullable: false),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    RegionNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_sea_region_assignment_audit_detail", x => new { x.AuditId, x.ChangeType, x.RegionId });
                    table.ForeignKey(
                        name: "FK_customer_sea_region_assignment_audit_detail_customer_sea_re~",
                        column: x => x.AuditId,
                        principalTable: "customer_sea_region_assignment_audit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document_version",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "文档版本标识"),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false, comment: "版本号（从 1 递增）"),
                    FileStorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "文件存储 Key"),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "原始文件名"),
                    FileSize = table.Column<long>(type: "bigint", nullable: false, comment: "文件大小（字节）"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false, comment: "文档ID")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_version", x => x.Id);
                    table.ForeignKey(
                        name: "FK_document_version_document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "document",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expense_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InvoiceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpenseClaimId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expense_item_expense_claim_ExpenseClaimId",
                        column: x => x.ExpenseClaimId,
                        principalTable: "expense_claim",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "订单分类标识"),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "订单ID"),
                    ProductCategoryId = table.Column<Guid>(type: "uuid", nullable: false, comment: "产品分类ID"),
                    CategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "产品分类名称"),
                    DiscountPoints = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false, comment: "优惠点数"),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, comment: "备注")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_category_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    InstallNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TrainingDuration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PackingStatus = table.Column<int>(type: "integer", nullable: false),
                    ReviewStatus = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Qty = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_item", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_item_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "order_remark",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "订单备注标识"),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false, comment: "订单ID"),
                    AddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "添加时间"),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "用户ID"),
                    TypeId = table.Column<int>(type: "integer", nullable: false, comment: "类型ID"),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false, comment: "说明内容")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_remark", x => x.Id);
                    table.ForeignKey(
                        name: "FK_order_remark_order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerContactId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Mobile = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    OfficePhone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    QQ = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Wechat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    Remark = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_contact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project_contact_project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_follow_up_record",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    VisitDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReminderIntervalDays = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatorId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_follow_up_record", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project_follow_up_record_project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_task_comment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AuthorId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProjectTaskId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_task_comment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project_task_comment_project_task_ProjectTaskId",
                        column: x => x.ProjectTaskId,
                        principalTable: "project_task",
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
                    IsDeptManager = table.Column<bool>(type: "boolean", nullable: false, comment: "是否为该部门主管"),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "创建时间"),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "完成时间"),
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
                name: "customer_contact_record_contact",
                columns: table => new
                {
                    RecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_contact_record_contact", x => new { x.RecordId, x.ContactId });
                    table.ForeignKey(
                        name: "FK_customer_contact_record_contact_customer_contact_record_Rec~",
                        column: x => x.RecordId,
                        principalTable: "customer_contact_record",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "customer_sea_visibility_entry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "可见性条目 ID"),
                    BoardId = table.Column<Guid>(type: "uuid", nullable: false, comment: "客户 ID"),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "被授权用户 ID"),
                    GrantedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "授权时间（UTC）"),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "撤回时间（UTC），空表示仍生效")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_sea_visibility_entry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_sea_visibility_entry_customer_sea_visibility_board~",
                        column: x => x.BoardId,
                        principalTable: "customer_sea_visibility_board",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_announcement_CreatedAt",
                table: "announcement",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_PublishAt",
                table: "announcement",
                column: "PublishAt");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_PublisherId",
                table: "announcement",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_Status",
                table: "announcement",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_announcement_read_record_AnnouncementId_UserId",
                table: "announcement_read_record",
                columns: new[] { "AnnouncementId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_announcement_read_record_UserId",
                table: "announcement_read_record",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_asset_Code",
                table: "asset",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_asset_Status",
                table: "asset",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_asset_allocation_AssetId",
                table: "asset_allocation",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_asset_allocation_UserId",
                table: "asset_allocation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_CheckInAt",
                table: "attendance_record",
                column: "CheckInAt");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_UserId",
                table: "attendance_record",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_attendance_record_UserId_CheckInAt",
                table: "attendance_record",
                columns: new[] { "UserId", "CheckInAt" });

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
                name: "IX_chat_group_CreatorId",
                table: "chat_group",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_group_Type",
                table: "chat_group",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_chat_group_member_ChatGroupId",
                table: "chat_group_member",
                column: "ChatGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_group_member_UserId",
                table: "chat_group_member",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_ChatGroupId",
                table: "chat_message",
                column: "ChatGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_CreatedAt",
                table: "chat_message",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_chat_message_SenderId",
                table: "chat_message",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_contact_CreatorId",
                table: "contact",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_contact_GroupId",
                table: "contact",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_contact_group_CreatorId",
                table: "contact_group",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_contract_Code",
                table: "contract",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_contract_CustomerId",
                table: "contract",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_contract_EndDate",
                table: "contract",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_contract_IsDeleted",
                table: "contract",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_contract_OrderId",
                table: "contract",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_contract_SignDate",
                table: "contract",
                column: "SignDate");

            migrationBuilder.CreateIndex(
                name: "IX_contract_Status",
                table: "contract",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_contract_invoice_ContractId",
                table: "contract_invoice",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_contract_type_option_SortOrder",
                table: "contract_type_option",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_contract_type_option_TypeValue",
                table: "contract_type_option",
                column: "TypeValue");

            migrationBuilder.CreateIndex(
                name: "IX_customer_CreatedAt",
                table: "customer",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_customer_FullName",
                table: "customer",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_customer_IsInSea",
                table: "customer",
                column: "IsInSea");

            migrationBuilder.CreateIndex(
                name: "IX_customer_OwnerId",
                table: "customer",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_contact_CustomerId",
                table: "customer_contact",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_contact_record_CustomerId",
                table: "customer_contact_record",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_contact_record_IsDeleted",
                table: "customer_contact_record",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_customer_contact_record_RecordAt",
                table: "customer_contact_record",
                column: "RecordAt");

            migrationBuilder.CreateIndex(
                name: "IX_customer_contact_record_contact_ContactId",
                table: "customer_contact_record_contact",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_industry_CustomerId_IndustryId",
                table: "customer_industry",
                columns: new[] { "CustomerId", "IndustryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_sea_region_assignment_TargetUserId",
                table: "customer_sea_region_assignment",
                column: "TargetUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_sea_region_assignment_audit_CreatedAt",
                table: "customer_sea_region_assignment_audit",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_customer_sea_region_assignment_audit_TargetUserId",
                table: "customer_sea_region_assignment_audit",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_sea_region_assignment_audit_detail_RegionId",
                table: "customer_sea_region_assignment_audit_detail",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_sea_region_assignment_region_RegionId",
                table: "customer_sea_region_assignment_region",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_sea_visibility_entry_BoardId_UserId",
                table: "customer_sea_visibility_entry",
                columns: new[] { "BoardId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customer_sea_visibility_entry_UserId",
                table: "customer_sea_visibility_entry",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_share_SharedToUserId",
                table: "customer_share",
                column: "SharedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_customer_source_SortOrder",
                table: "customer_source",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_customer_source_UsageScene",
                table: "customer_source",
                column: "UsageScene");

            migrationBuilder.CreateIndex(
                name: "IX_dept_IsDeleted",
                table: "dept",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_dept_ManagerId",
                table: "dept",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_dept_ParentId",
                table: "dept",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_dept_Status",
                table: "dept",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_document_CreatorId",
                table: "document",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_document_version_DocumentId",
                table: "document_version",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_claim_ApplicantId",
                table: "expense_claim",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_claim_CreatedAt",
                table: "expense_claim",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_expense_claim_Status",
                table: "expense_claim",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_expense_item_ExpenseClaimId",
                table: "expense_item",
                column: "ExpenseClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_income_expense_type_option_SortOrder",
                table: "income_expense_type_option",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_income_expense_type_option_TypeValue",
                table: "income_expense_type_option",
                column: "TypeValue");

            migrationBuilder.CreateIndex(
                name: "IX_industry_ParentId",
                table: "industry",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_leave_balance_UserId_Year_LeaveType",
                table: "leave_balance",
                columns: new[] { "UserId", "Year", "LeaveType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_leave_request_ApplicantId",
                table: "leave_request",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_leave_request_CreatedAt",
                table: "leave_request",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_leave_request_Status",
                table: "leave_request",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_leave_request_WorkflowInstanceId",
                table: "leave_request",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_booking_BookerId",
                table: "meeting_booking",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_booking_MeetingRoomId",
                table: "meeting_booking",
                column: "MeetingRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_booking_StartAt",
                table: "meeting_booking",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_meeting_room_Status",
                table: "meeting_room",
                column: "Status");

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
                name: "IX_order_CreatedAt",
                table: "order",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_order_CustomerId",
                table: "order",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_order_DeptId",
                table: "order",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_order_IsDeleted",
                table: "order",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_order_OrderLogisticsCompanyId",
                table: "order",
                column: "OrderLogisticsCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_order_OrderLogisticsMethodId",
                table: "order",
                column: "OrderLogisticsMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_order_OrderNumber",
                table: "order",
                column: "OrderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_order_OwnerId",
                table: "order",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_order_ProjectId",
                table: "order",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_order_WaybillNumber",
                table: "order",
                column: "WaybillNumber");

            migrationBuilder.CreateIndex(
                name: "IX_order_WorkflowInstanceId",
                table: "order",
                column: "WorkflowInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_order_category_OrderId",
                table: "order_category",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_order_category_OrderId_ProductCategoryId",
                table: "order_category",
                columns: new[] { "OrderId", "ProductCategoryId" });

            migrationBuilder.CreateIndex(
                name: "IX_order_category_ProductCategoryId",
                table: "order_category",
                column: "ProductCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_OrderId",
                table: "order_item",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_order_item_ProductId",
                table: "order_item",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_order_logistics_company_Sort",
                table: "order_logistics_company",
                column: "Sort");

            migrationBuilder.CreateIndex(
                name: "IX_order_logistics_company_TypeValue",
                table: "order_logistics_company",
                column: "TypeValue");

            migrationBuilder.CreateIndex(
                name: "IX_order_logistics_method_Sort",
                table: "order_logistics_method",
                column: "Sort");

            migrationBuilder.CreateIndex(
                name: "IX_order_logistics_method_TypeValue",
                table: "order_logistics_method",
                column: "TypeValue");

            migrationBuilder.CreateIndex(
                name: "IX_order_remark_AddedAt",
                table: "order_remark",
                column: "AddedAt");

            migrationBuilder.CreateIndex(
                name: "IX_order_remark_OrderId",
                table: "order_remark",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_order_remark_TypeId",
                table: "order_remark",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_order_remark_UserId",
                table: "order_remark",
                column: "UserId");

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
                name: "IX_product_Barcode",
                table: "product",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_product_CategoryId",
                table: "product",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_product_Code",
                table: "product",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_product_ProductTypeId",
                table: "product",
                column: "ProductTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_product_SupplierId",
                table: "product",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_product_category_ParentId",
                table: "product_category",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_product_category_SortOrder",
                table: "product_category",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_product_parameter_ProductId",
                table: "product_parameter",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_parameter_ProductId_Year",
                table: "product_parameter",
                columns: new[] { "ProductId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_type_SortOrder",
                table: "product_type",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_project_CreatorId",
                table: "project",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_project_CustomerId",
                table: "project",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_project_ProjectIndustryId",
                table: "project",
                column: "ProjectIndustryId");

            migrationBuilder.CreateIndex(
                name: "IX_project_Status",
                table: "project",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_project_contact_ProjectId",
                table: "project_contact",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_project_follow_up_record_ProjectId",
                table: "project_follow_up_record",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_project_follow_up_record_VisitDate",
                table: "project_follow_up_record",
                column: "VisitDate");

            migrationBuilder.CreateIndex(
                name: "IX_project_industry_SortOrder",
                table: "project_industry",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_project_status_option_SortOrder",
                table: "project_status_option",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_AssigneeId",
                table: "project_task",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_ProjectId",
                table: "project_task",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_Status",
                table: "project_task",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_project_task_comment_ProjectTaskId",
                table: "project_task_comment",
                column: "ProjectTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_project_type_SortOrder",
                table: "project_type",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_region_Level",
                table: "region",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_region_ParentId",
                table: "region",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_role_Name",
                table: "role",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_UserId",
                table: "schedule",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_schedule_UserId_WorkDate",
                table: "schedule",
                columns: new[] { "UserId", "WorkDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_schedule_WorkDate",
                table: "schedule",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_share_link_DocumentId",
                table: "share_link",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_share_link_Token",
                table: "share_link",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_FullName",
                table: "supplier",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_user_Email",
                table: "user",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_user_Name",
                table: "user",
                column: "Name");

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
                name: "IX_vehicle_PlateNumber",
                table: "vehicle",
                column: "PlateNumber");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_Status",
                table: "vehicle",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_booking_BookerId",
                table: "vehicle_booking",
                column: "BookerId");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_booking_StartAt",
                table: "vehicle_booking",
                column: "StartAt");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_booking_VehicleId",
                table: "vehicle_booking",
                column: "VehicleId");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "announcement");

            migrationBuilder.DropTable(
                name: "announcement_read_record");

            migrationBuilder.DropTable(
                name: "asset");

            migrationBuilder.DropTable(
                name: "asset_allocation");

            migrationBuilder.DropTable(
                name: "attendance_record");

            migrationBuilder.DropTable(
                name: "CAPLock");

            migrationBuilder.DropTable(
                name: "CAPPublishedMessage");

            migrationBuilder.DropTable(
                name: "CAPReceivedMessage");

            migrationBuilder.DropTable(
                name: "chat_group_member");

            migrationBuilder.DropTable(
                name: "chat_message");

            migrationBuilder.DropTable(
                name: "contact");

            migrationBuilder.DropTable(
                name: "contact_group");

            migrationBuilder.DropTable(
                name: "contract_invoice");

            migrationBuilder.DropTable(
                name: "contract_type_option");

            migrationBuilder.DropTable(
                name: "customer_contact");

            migrationBuilder.DropTable(
                name: "customer_contact_record_contact");

            migrationBuilder.DropTable(
                name: "customer_industry");

            migrationBuilder.DropTable(
                name: "customer_sea_region_assignment_audit_detail");

            migrationBuilder.DropTable(
                name: "customer_sea_region_assignment_region");

            migrationBuilder.DropTable(
                name: "customer_sea_visibility_entry");

            migrationBuilder.DropTable(
                name: "customer_share");

            migrationBuilder.DropTable(
                name: "customer_source");

            migrationBuilder.DropTable(
                name: "dept");

            migrationBuilder.DropTable(
                name: "document_version");

            migrationBuilder.DropTable(
                name: "expense_item");

            migrationBuilder.DropTable(
                name: "income_expense_type_option");

            migrationBuilder.DropTable(
                name: "industry");

            migrationBuilder.DropTable(
                name: "leave_balance");

            migrationBuilder.DropTable(
                name: "leave_request");

            migrationBuilder.DropTable(
                name: "meeting_booking");

            migrationBuilder.DropTable(
                name: "meeting_room");

            migrationBuilder.DropTable(
                name: "notification");

            migrationBuilder.DropTable(
                name: "operation_log");

            migrationBuilder.DropTable(
                name: "order_category");

            migrationBuilder.DropTable(
                name: "order_invoice_type_option");

            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "order_logistics_company");

            migrationBuilder.DropTable(
                name: "order_logistics_method");

            migrationBuilder.DropTable(
                name: "order_remark");

            migrationBuilder.DropTable(
                name: "position");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "product_category");

            migrationBuilder.DropTable(
                name: "product_parameter");

            migrationBuilder.DropTable(
                name: "product_type");

            migrationBuilder.DropTable(
                name: "project_contact");

            migrationBuilder.DropTable(
                name: "project_follow_up_record");

            migrationBuilder.DropTable(
                name: "project_industry");

            migrationBuilder.DropTable(
                name: "project_status_option");

            migrationBuilder.DropTable(
                name: "project_task_comment");

            migrationBuilder.DropTable(
                name: "project_type");

            migrationBuilder.DropTable(
                name: "region");

            migrationBuilder.DropTable(
                name: "role_data_dept");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "schedule");

            migrationBuilder.DropTable(
                name: "share_link");

            migrationBuilder.DropTable(
                name: "supplier");

            migrationBuilder.DropTable(
                name: "user_dept");

            migrationBuilder.DropTable(
                name: "user_position");

            migrationBuilder.DropTable(
                name: "user_refresh_token");

            migrationBuilder.DropTable(
                name: "user_role");

            migrationBuilder.DropTable(
                name: "vehicle");

            migrationBuilder.DropTable(
                name: "vehicle_booking");

            migrationBuilder.DropTable(
                name: "workflow_definition");

            migrationBuilder.DropTable(
                name: "workflow_task");

            migrationBuilder.DropTable(
                name: "chat_group");

            migrationBuilder.DropTable(
                name: "contract");

            migrationBuilder.DropTable(
                name: "customer_contact_record");

            migrationBuilder.DropTable(
                name: "customer_sea_region_assignment_audit");

            migrationBuilder.DropTable(
                name: "customer_sea_region_assignment");

            migrationBuilder.DropTable(
                name: "customer_sea_visibility_board");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "expense_claim");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "project_task");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropTable(
                name: "workflow_instance");

            migrationBuilder.DropTable(
                name: "customer");
        }
    }
}
