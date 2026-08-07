

# Ncp.Admin

Platform scaffold based on [NetCorePal Cloud Framework](https://github.com/netcorepal/netcorepal-cloud-framework): IAM (Users/Roles/Departments/Positions) + Workflow Engine + Platform Infrastructure (Notifications, Logging, File Management, Dashboard). Backend: ASP.NET Core + DDD. Frontend: [Vben Admin](https://github.com/vbenjs/vue-vben-admin) (Vue 3 + Vite + TypeScript + Ant Design Vue).

> **Note**: This branch has removed original business modules such as CRM / Administrative OA. When extending business functionality, please refer to the complete implementation in the Git history.

### Default Administrator (Auto-seeded on first startup with an empty database)

| Item | Value |
|---|---|
| Username | `admin` |
| Password | `Admin@123456` |

Immediately change the password in production environments. The database uses a single baseline migration `InitPlatform` (incompatible with legacy business databases, please create a new database).

---

## Project Preview

The following are screenshots of the frontend admin interface.

**Login Page**

![Login Page](docs/imgs/login.png)

**Dashboard / Data Analytics**

![Data Analytics](docs/imgs/analytics.png)

**Department Management**

![Department Management](docs/imgs/dept.png)

**Role Management**

![Role Management](docs/imgs/role.png)

**API Documentation (Scalar)**

![Scalar API](docs/imgs/scalar-api.png)

---

## Environment Setup

### Using Aspire (Recommended)

The project enables Aspire support. Only a Docker environment is required; no manual configuration of infrastructure services is needed.

```bash
# Ensure Docker is running
docker version

# Run the AppHost project directly. Aspire will automatically manage all dependencies
cd src/Ncp.Admin.AppHost
dotnet run
```

Aspire will automatically:
- Start and manage database containers (this template uses **PostgreSQL**)
- Start and manage message queue containers (RabbitMQ, Kafka, NATS, etc.)
- Start and manage Redis containers
- Provide a unified Aspire Dashboard UI to view all service statuses
- Automatically configure connection strings and dependencies between services

Access the Aspire Dashboard (typically at http://localhost:15888) to view and manage all services.

### Recommended: Using Initialization Scripts (Without Aspire)

If Aspire is not enabled, the project provides comprehensive infrastructure initialization scripts for rapid development environment setup:

#### Using Docker Compose (Recommended)
```bash
# Enter the scripts directory
cd scripts

# Start default infrastructure (PostgreSQL + Redis + RabbitMQ, recommended for this template)
docker-compose --profile postgres up -d

# Start only Redis + RabbitMQ, using MySQL as the database (requires manual connection string configuration)
docker-compose up -d

# Use SQL Server instead of PostgreSQL
docker-compose --profile sqlserver up -d

# Use Kafka instead of RabbitMQ
docker-compose --profile kafka up -d

# Stop all services
docker-compose down

# Stop and remove data volumes (complete cleanup)
docker-compose down -v
```

#### Using Initialization Scripts
```bash
# Linux/macOS
cd scripts
./init-infrastructure.sh

# Windows PowerShell
cd scripts
.\init-infrastructure.ps1

# Clean up environment
./clean-infrastructure.sh        # Linux/macOS
.\clean-infrastructure.ps1       # Windows
```

### Manual Method: Running Docker Containers Individually

If you need manual control over each container, use the following commands:

```bash
# Redis
docker run --restart unless-stopped --name netcorepal-redis -p 6379:6379 -v netcorepal_redis_data:/data -d redis:7.2-alpine redis-server --appendonly yes --databases 1024

# PostgreSQL (Default database for this template)
docker run --restart unless-stopped --name netcorepal-postgres -p 5432:5432 -e POSTGRES_PASSWORD=123456 -e TZ=Asia/Shanghai -v netcorepal_postgres_data:/var/lib/postgresql/data -d postgres:16-alpine

# RabbitMQ
docker run --restart unless-stopped --name netcorepal-rabbitmq -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest -v netcorepal_rabbitmq_data:/var/lib/rabbitmq -d rabbitmq:3.12-management-alpine
```

### Service Access Information

After startup, access services via the following addresses:

- **Frontend App**: http://localhost:5666/
- **API Docs (Scalar)**: http://localhost:5511/scalar (available after backend starts)
- **Redis**: `localhost:6379`
- **PostgreSQL**: `localhost:5432` (postgres/123456) (default for this template)
- **RabbitMQ AMQP**: `localhost:5672` (guest/guest)
- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)
- **SQL Server**: `localhost:1433` (sa/Test123456!) (optional profile)
- **MySQL**: `localhost:3306` (root/123456) (optional profile)
- **Kafka**: `localhost:9092`
- **Kafka UI**: http://localhost:8080

## Frontend Project Setup

The frontend is based on [Vben Admin](https://github.com/vbenjs/vue-vben-admin), using Vue 3 + Vite + TypeScript + Ant Design Vue. Refer to the **Project Preview** above for interface screenshots.

### Requirements

- **Node.js**: >= 20.12.0
- **pnpm**: >= 10.0.0

### Install Dependencies

```bash
# Enter the frontend directory
cd src/frontend

# Install dependencies
npm i -g corepack
pnpm install
```

### Start Development Server

```bash
# Run inside the frontend directory
pnpm dev:antd
```

After successful startup, the frontend app will run at **http://localhost:5666**.

### Build for Production

```bash
# Run inside the frontend directory
pnpm build:antd
```

The build artifacts will be output to the `frontend/apps/admin-antd/dist` directory.

### Other Common Commands

```bash
# Code linting
pnpm lint

# Code formatting
pnpm format

# Type checking
pnpm check:type

# Preview build results
pnpm preview
```

### Environment Variable Configuration

The frontend environment variable configuration file is located at `frontend/apps/admin-antd/.env.development`:

- `VITE_PORT`: Development server port (default: 5666)
- `VITE_GLOB_API_URL`: Backend API address (default: http://localhost:5511/api)
- `VITE_NITRO_MOCK`: Enable Mock service (default: false)

## IDE Code Snippet Configuration

This template provides extensive code snippets to help you quickly generate common code structures.

### Visual Studio Configuration

Run the following PowerShell command to auto-install code snippets:

```powershell
cd vs-snippets
.\Install-VSSnippets.ps1
```

Or install manually:

1. Open Visual Studio
2. Navigate to `Tools` > `Code Snippets Manager`
3. Import the `vs-snippets/NetCorePalTemplates.snippet` file

### VS Code Configuration

VS Code code snippets are pre-configured in the `.vscode/csharp.code-snippets` file and apply automatically when the project is opened.

### JetBrains Rider Configuration

Rider users can directly use the Live Templates configuration in the `Ncp.Admin.sln.DotSettings` file.

### Available Code Snippets

#### NetCorePal (ncp) Shortcuts
| Shortcut | Description | Generated Content |
|----------|-------------|-------------------|
| `ncpcmd` | NetCorePal Command | ICommand implementation (includes validator and handler) |
| `ncpcmdres` | Command (with return value) | ICommand&lt;Response&gt; implementation |
| `ncpar` | Aggregate Root | Entity&lt;Id&gt; and IAggregateRoot |
| `ncprepo` | NetCorePal Repository | IRepository interface and implementation |
| `ncpie` | Integration Event | IntegrationEvent and handler |
| `ncpdeh` | Domain Event Handler | IDomainEventHandler implementation |
| `ncpiec` | Integration Event Converter | IIntegrationEventConverter |
| `ncpde` | Domain Event | IDomainEvent record |

#### Endpoint (ep) Shortcuts
| Shortcut | Description | Generated Content |
|----------|-------------|-------------------|
| `epp` | FastEndpoint (NCP Style) | Complete vertical slice implementation |
| `epreq` | Request-only Endpoint | Endpoint&lt;Request&gt; |
| `epres` | Response-only Endpoint | EndpointWithoutRequest&lt;Response&gt; |
| `epdto` | Endpoint DTOs | Request and Response classes |
| `epval` | Endpoint Validator | Validator&lt;Request&gt; |
| `epmap` | Endpoint Mapper | Mapper&lt;Request, Response, Entity&gt; |
| `epfull` | Complete Endpoint Slice | Full implementation with mapper |
| `epsum` | Endpoint Summary | Summary&lt;Endpoint, Request&gt; |
| `epnoreq` | Request-less Endpoint | EndpointWithoutRequest |
| `epreqres` | Request-Response Endpoint | Endpoint&lt;Request, Response&gt; |
| `epdat` | Endpoint Data | Static data class |

For more detailed configuration, refer to: [vs-snippets/README.md](vs-snippets/README.md)

## Dependencies & Frameworks

+ [NetCorePal Cloud Framework](https://github.com/netcorepal/netcorepal-cloud-framework)
+ [ASP.NET Core](https://github.com/dotnet/aspnetcore)
+ [EFCore](https://github.com/dotnet/efcore)
+ [CAP](https://github.com/dotnetcore/CAP)
+ [MediatR](https://github.com/jbogard/MediatR)
+ [FluentValidation](https://docs.fluentvalidation.net/en/latest)
+ [Swashbuckle.AspNetCore.Swagger](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

## Database Migration

This template uses **PostgreSQL**. Ensure that `appsettings.json` or the environment's `ConnectionStrings:PostgreSQL` is configured with a valid connection string (injected automatically by AppHost when using Aspire).

```shell
# Install tool SEE： https://learn.microsoft.com/zh-cn/ef/core/cli/dotnet#installing-the-tools
dotnet tool install --global dotnet-ef --version 9.0.0

# Update database (requires specifying startup project to load connection strings)
dotnet ef database update -p src/Ncp.Admin.Infrastructure -s src/Ncp.Admin.Web

# Create migration SEE：https://learn.microsoft.com/zh-cn/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli
dotnet ef migrations add YourMigrationName -p src/Ncp.Admin.Infrastructure -s src/Ncp.Admin.Web
```

## Code Analysis Visualization

The framework provides powerful code flow analysis and visualization capabilities, helping developers intuitively understand component relationships and data flow within the DDD architecture.

### 🎯 Core Features

+ **Automatic Code Analysis**: Automatically analyzes code structure via source generators, identifying controllers, commands, aggregate roots, events, and other components
+ **Multiple Chart Types**: Supports architecture flowcharts, command chain diagrams, event flow diagrams, class diagrams, and more
+ **Interactive HTML Visualization**: Generates complete interactive HTML pages with built-in navigation and chart previews
+ **One-Click Online Editing**: Integrates a "View in Mermaid Live" button for direct jumping to the online editor

### 🚀 Quick Start

Install the CLI tool to generate standalone HTML files:

```bash
# Install global tool
dotnet tool install -g NetCorePal.Extensions.CodeAnalysis.Tools

# Enter the project directory and generate the visualization file
cd src/Ncp.Admin.Web
netcorepal-codeanalysis generate --output architecture.html
```

### ✨ Main Features

+ **Interactive HTML Page**:
  + Left tree navigation with support for switching between different chart types
  + Built-in Mermaid.js real-time rendering
  + Responsive design adapted to different devices
  + Professional modern interface

+ **One-Click Online Editing**:
  + "View in Mermaid Live" button in the top-right corner of each chart
  + Intelligent compression algorithm to optimize URL length
  • Automatically jumps to [Mermaid Live Editor](https://mermaid.live/)
  + Supports online editing, image export, and sharing link generation

### 📖 Detailed Documentation

For complete usage instructions and examples, please refer to:

+ [Code Flow Analysis Documentation](https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-flow-analysis/)
+ [Code Analysis Tools Documentation](https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-analysis-tools/)

## Monitoring

This project uses `prometheus-net` as the monitoring solution for integration with infrastructure Prometheus, exposing metrics by default at the `/metrics` endpoint.

For more information, see: [https://github.com/prometheus-net/prometheus-net](https://github.com/prometheus-net/prometheus-net)

## Cursor Prompt Examples (Copyable)

In the Cursor input box, type **`/`** to select a skill from this repository's [`.cursor/skills/`](.cursor/skills/) directory (e.g., `cleanddd-requirements-analysis`). **In the same message**, clearly specify the task and context. To enforce repository conventions, **`@`** [`.cursor/rules/project-conventions.mdc`](.cursor/rules/project-conventions.mdc) or [`.cursor/rules/frontend-vben.mdc`](.cursor/rules/frontend-vben.mdc).

### `.cursor/skills` Usage Guide (Which one to pick under `/`)

| Skill | Path | Purpose | Typical Usage |
|-------|------|---------|---------------|
| **cleanddd-requirements-analysis** | [SKILL.md](.cursor/skills/cleanddd-requirements-analysis/SKILL.md) | Breaks business requirements into structured descriptions (stakeholders, items, target entities), **does not directly model** | When adding new features/requirements, first clarify "what to do" |
| **cleanddd-modeling** | [SKILL.md](.cursor/skills/cleanddd-modeling/SKILL.md) | Produces aggregation, command, query, event, Endpoint, etc. **modeling blueprints** | When requirements are clear, define boundaries and interface shapes before coding |
| **cleanddd-dotnet-coding** | [SKILL.md](.cursor/skills/cleanddd-dotnet-coding/SKILL.md) | Implements Domain / Infrastructure / Web (commands, queries, endpoints, configs, tests) **in this repo** | Writing backend code, making changes, or reviewing DDD layering |
| **cleanddd-coach** | [SKILL.md](.cursor/skills/cleanddd-coach/SKILL.md) | **Learning & Coaching**: Aggregate boundaries, CQRS, events, anti-patterns (micro-lessons/quizzes/checklists) | Understanding concepts or aligning team terminology, not for immediate feature implementation |
| **ncp-admin-frontend** | [SKILL.md](.cursor/skills/ncp-admin-frontend/SKILL.md) | **Vben Admin Frontend** (Vue3 + Ant Design): Pages, APIs, routing, i18n, permission alignment | Modifying `src/frontend/apps/admin-antd` or integrating with backend permissions |

**Mnemonic**: First **requirements** clarification → **modeling** blueprint → **dotnet-coding** backend; use **ncp-admin-frontend** for frontend; use **coach** for theory.

### Example Prompts per Skill (Copyable)

**Usage (always use slash)**: Type **`/`** → select skill name → paste or modify the template below in the **same message** body. If repository norms are needed, start a new line or use **`@`** `project-conventions.mdc` etc. in the same message.

---

**Requirement Breakdown (`cleanddd-requirements-analysis`) — Standard Template**

1. Type **`/cleanddd-requirements-analysis`** (or select it from the `/` list).
2. Use the template below in the message body (replace bracketed content with your actual info; write "N/A" or remove the section if not applicable).

```text
【Scope & Background】
- System/Module: (e.g., Admin Panel · Order-related)
- Current Goal: (e.g., Add "Delayed Shipment Registration" capability)
- Out of Scope / Not Doing: (e.g., No payment changes, no third-party logistics API integration)

【Business Narrative】
(Clearly describe in natural language: who, under what conditions, does what, and what results they see; use bullet points if needed.)

【Known Constraints】
- Roles & Permissions: (Write if known, otherwise "Pending confirmation")
- Relationship to existing features: (e.g., Only allowed when order status is "Ordered")
- External systems / Data dependencies: (Write "None" if applicable)

【Delivery Requirements】
Please strictly follow the output format of the cleanddd-requirements-analysis skill to provide structured Markdown, and:
- Only perform requirement-level breakdown; do not use modeling terms like aggregates/commands/domain events;
- Must include: Stakeholder table, Requirement items table, Business entity view, Trigger/subsequent actions table, Assumptions & pending confirmation checklist;
- End with a "Parameter Summary + Whether to proceed to downstream modeling" confirmation prompt.
```

---

**Domain Modeling (`cleanddd-modeling`)**

```text
/cleanddd-modeling

【Input】The following content is the confirmed requirement breakdown (paste analysis/requirements.md or table from chat).

(Paste: Stakeholder table, Requirement items table, etc.)

Please output according to skill conventions: Aggregates, Commands, Queries, Domain Events, Endpoints, (if applicable) Scheduled Tasks; list unresolved issues at the end.
```

**Backend Implementation in this Repo (`cleanddd-dotnet-coding`)**

```text
/cleanddd-dotnet-coding
@.cursor/rules/project-conventions.mdc

【Task】(Specify the aggregate, endpoint to modify, or new capability)
【Basis】(Paste modeling summary or interface list if available)
```

**Frontend (`ncp-admin-frontend`)**

```text
/ncp-admin-frontend
@.cursor/rules/frontend-vben.mdc

【Page/Menu】……
【API Integration】……
```

**CleanDDD Coach (`cleanddd-coach`)**

```text
/cleanddd-coach Please use the "Aggregates & Invariants" module, combined with our order scenario, to help me define boundaries and provide a checklist.
```

### Review a Module for DDD / CleanDDD Compliance

```text
/cleanddd-dotnet-coding
@.cursor/rules/project-conventions.mdc
@ModulePath

Please review whether the specified module complies with DDD/CleanDDD according to the above conventions:

- Whether aggregate boundaries and invariants are maintained within the aggregate
- Whether there are cross-aggregate direct state modifications (should use domain events, etc.)
- Whether Command / Query / Endpoint / Repository / Entity configuration layers are correct
- Whether command handlers avoid explicit SaveCalls and whether exceptions use KnownException, etc.

Please output: Compliant items, Risk items, Suggested refactoring points (if any).
```

### Add New Aggregate (Example: Order)

```text
/cleanddd-dotnet-coding
@.cursor/rules/project-conventions.mdc

Add the Order aggregate in this repo. If there are existing modeling conclusions, paste them below under 【Modeling Basis】.

- Domain: Aggregate root, strongly-typed OrderId, necessary entities/value objects, domain events
- Infrastructure: IOrderRepository implementation, entity configuration, DbContext registration
- Web: Related Command / Query / Validator / Handler, Endpoints

If endpoints require authentication, list the permission-related locations that must be updated synchronously (see "Backend 5 Locations" in project-conventions).

【Modeling Basis】(Optional, from /cleanddd-modeling output summary)
```

(Recommended workflow: First `/cleanddd-requirements-analysis` → then `/cleanddd-modeling` → finally `/cleanddd-dotnet-coding`.)
