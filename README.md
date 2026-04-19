# Blind-Match PAS

Blind-Match PAS is a production-style ASP.NET Core 8 MVC academic web application for the PUSL2020 Software Development Tools and Practices coursework. It implements a secure Project Approval System with blind supervisor review, controlled identity reveal after confirmed match, role-based dashboards, auditability, and test coverage.

## Tech Stack

- ASP.NET Core 8 MVC
- C#
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Razor Views
- Bootstrap 5 with a custom premium theme
- xUnit, Moq, integration testing with SQLite/WebApplicationFactory

## Solution Structure

- `BlindMatchPAS.sln`: solution file
- `BlindMatchPAS.Web`: main MVC application
- `BlindMatchPAS.UnitTests`: unit tests for service/business rules
- `BlindMatchPAS.IntegrationTests`: repository and route protection integration tests
- `docs`: coursework support notes

## Key Features

- Student proposal submission, edit, withdrawal, and lifecycle tracking
- Anonymous supervisor proposal browsing before identity reveal
- Supervisor interest expression and match confirmation
- Admin oversight dashboards, user management, research area management, audit logs, and reassignment
- Seeded demo users, demo proposals, and migration-ready EF Core setup
- Clean service/repository structure and RBAC with ASP.NET Core Identity

## Prerequisites

- Visual Studio 2022 or later with `.NET 8` workload
- .NET SDK 8.x
- SQL Server or SQL Server Express / LocalDB equivalent
- Optional: EF Core CLI via `dotnet-ef`

## Open in Visual Studio

1. Open Visual Studio.
2. Choose `Open a project or solution`.
3. Open `BlindMatchPAS.sln`.
4. Set `BlindMatchPAS.Web` as the startup project.

## Configure the Connection String

Update the SQL Server connection string in:

- `BlindMatchPAS.Web/appsettings.json`
- `BlindMatchPAS.Web/appsettings.Development.json`

Default format used in the project:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BlindMatchPASDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Replace the server, database, username, and password values to match your environment.

## Run Migrations

The project includes an initial migration in `BlindMatchPAS.Web/Data/Migrations`.

Option 1, Visual Studio Package Manager Console:

```powershell
Update-Database -Project BlindMatchPAS.Web -StartupProject BlindMatchPAS.Web
```

Option 2, .NET CLI:

```bash
~/.dotnet/tools/dotnet-ef database update --project BlindMatchPAS.Web --startup-project BlindMatchPAS.Web
```

## Seed the Database

Database seeding runs automatically on application startup outside the `Testing` environment after migrations are applied.

Seeded content includes:

- research areas
- student accounts
- supervisor accounts
- module leader account
- system admin account
- sample pending proposal
- sample under-review proposal
- sample matched proposal

## Default Login Accounts

All seeded demo accounts use:

- Password: `P@ssword123!`

Seeded users:

- System Admin: `admin@blindmatchpas.local`
- Module Leader: `moduleleader@blindmatchpas.local`
- Supervisor 1: `supervisor.ai@blindmatchpas.local`
- Supervisor 2: `supervisor.cyber@blindmatchpas.local`
- Supervisor 3: `supervisor.iot@blindmatchpas.local`
- Student 1: `student.group1@blindmatchpas.local`
- Student 2: `student.group2@blindmatchpas.local`
- Student 3: `student.group3@blindmatchpas.local`

## Run the Application

From Visual Studio:

1. Build the solution.
2. Ensure the database connection is valid.
3. Run `BlindMatchPAS.Web`.
4. Log in using one of the seeded accounts above.

From CLI:

```bash
dotnet run --project BlindMatchPAS.Web
```

## Run Tests

Run the full test suite:

```bash
dotnet test BlindMatchPAS.sln
```

Generate a coverage-friendly run:

```bash
dotnet test BlindMatchPAS.UnitTests/BlindMatchPAS.UnitTests.csproj --collect:"XPlat Code Coverage"
dotnet test BlindMatchPAS.IntegrationTests/BlindMatchPAS.IntegrationTests.csproj --collect:"XPlat Code Coverage"
```

## Project Structure Summary

- `Controllers`: thin MVC controllers for student, supervisor, admin, home, and account flows
- `Data`: `ApplicationDbContext`, seed data, and migrations
- `Models`: entities, enums, and shared service result models
- `Repositories`: repository abstractions and EF-backed implementations
- `Services`: business rules, blind matching logic, and admin orchestration
- `Utilities`: helper extensions and UI status mapping
- `ViewModels`: form, table, dashboard, and oversight view models
- `Views`: role-specific Razor views with custom themed layout

## Additional Documentation

- `DEVELOPMENT_NOTES.md`
- `docs/SAMPLE_REPORT_SUPPORT_NOTES.md`
- `docs/SUGGESTED_SCREENSHOTS.md`
- `docs/SUGGESTED_COMMIT_PLAN.md`
Environment setup completed using Visual Studio 2022 and SQL Server LocalDB.