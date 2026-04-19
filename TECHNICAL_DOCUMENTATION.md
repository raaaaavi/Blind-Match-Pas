# BlindMatchPAS Technical Documentation

## Quick Start

1. Open `BlindMatchPAS.sln` in Visual Studio or your preferred IDE.
2. Update the SQL Server connection string in:
   - `BlindMatchPAS.Web/appsettings.json`
   - `BlindMatchPAS.Web/appsettings.Development.json`
3. Run EF Core migrations.
4. Start the `BlindMatchPAS.Web` application.
5. Log in using one of the seeded demo accounts.

> This document is designed to help you set up, run, and understand the repository without changing the code.

## What is BlindMatchPAS?

BlindMatchPAS is an ASP.NET Core 8 MVC web app for academic project approvals. It lets students submit proposals, supervisors browse proposals anonymously, and admins monitor the approval process.

## What is included in the repository?

- `BlindMatchPAS.sln` - solution file.
- `BlindMatchPAS.Web/` - main web application.
- `BlindMatchPAS.UnitTests/` - unit tests.
- `BlindMatchPAS.IntegrationTests/` - integration tests.
- `docs/` - supporting documentation.

### Main folders in `BlindMatchPAS.Web`

- `Controllers/` - handles web requests and routes.
- `Data/` - EF Core database context, migrations, and seeding.
- `Models/` - domain entities, enums, and view models.
- `Repositories/` - data access logic.
- `Services/` - application business rules.
- `Utilities/` - helper extensions.
- `ViewModels/` - data structures for views.
- `Views/` - Razor pages and UI templates.

## Prerequisites

- .NET SDK 8.x installed.
- Visual Studio 2022 or newer, or another C# IDE.
- SQL Server, SQL Server Express, or LocalDB.
- Optional: `dotnet-ef` CLI tool.

## Setup steps

### 1. Open the project

Open `BlindMatchPAS.sln` in Visual Studio or your IDE.

### 2. Configure the database

Edit the connection string in both:

- `BlindMatchPAS.Web/appsettings.json`
- `BlindMatchPAS.Web/appsettings.Development.json`

Use a connection string like this:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BlindMatchPASDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Update the server name, database name, username, and password to match your environment.

### 3. Apply database migrations

This project includes EF Core migrations in `BlindMatchPAS.Web/Data/Migrations`.

Choose one option:

- Visual Studio Package Manager Console:

```powershell
Update-Database -Project BlindMatchPAS.Web -StartupProject BlindMatchPAS.Web
```

- .NET CLI:

```bash
dotnet ef database update --project BlindMatchPAS.Web --startup-project BlindMatchPAS.Web
```

### 4. Seed data automatically

Seed data runs when the app starts in non-testing environments after migrations complete. You do not need to run a separate command.

Seeded data includes:

- research areas
- demo student, supervisor, module leader, and admin accounts
- sample proposals

## Run the application

### Option A: Visual Studio

1. Set `BlindMatchPAS.Web` as the startup project.
2. Build the solution.
3. Run the application.
4. Log in with a demo account.

### Option B: CLI

```bash
dotnet run --project BlindMatchPAS.Web
```

## Demo login accounts

All seeded demo accounts use the password:

`P@ssword123!`

- System Admin: `admin@blindmatchpas.local`
- Module Leader: `moduleleader@blindmatchpas.local`
- Supervisor 1: `supervisor.ai@blindmatchpas.local`
- Supervisor 2: `supervisor.cyber@blindmatchpas.local`
- Supervisor 3: `supervisor.iot@blindmatchpas.local`
- Student 1: `student.group1@blindmatchpas.local`
- Student 2: `student.group2@blindmatchpas.local`
- Student 3: `student.group3@blindmatchpas.local`

## Who uses this app?

### Student

- Submit or edit proposals.
- Withdraw proposals.
- Track proposal status.
- See supervisor details only after a confirmed match.

### Supervisor

- Browse anonymous proposals.
- Express interest in proposals.
- Confirm matched proposals.

### Admin

- Manage users and research areas.
- Review audit logs.
- Reassign proposals.
- Monitor system status.

## Run tests

### Run all tests

```bash
dotnet test BlindMatchPAS.sln
```

### Run unit tests only

```bash
dotnet test BlindMatchPAS.UnitTests/BlindMatchPAS.UnitTests.csproj
```

### Run integration tests only

```bash
dotnet test BlindMatchPAS.IntegrationTests/BlindMatchPAS.IntegrationTests.csproj
```

## Notes for developers

- Authentication and roles are provided by ASP.NET Core Identity.
- Business logic is implemented in `Services/`.
- Data access uses repository patterns in `Repositories/`.
- Entities are defined in `Models/DomainEntities.cs`.
- Views are Razor templates styled with Bootstrap.
- Important actions are recorded in audit logs.

## Helpful resources

- `ARCHITECTURE_AND_USER_GUIDE.md` - architecture overview.
- `DEVELOPMENT_NOTES.md` - development-specific details.
- `docs/SAMPLE_REPORT_SUPPORT_NOTES.md` - report guidance.
- `docs/SUGGESTED_SCREENSHOTS.md` - screenshot suggestions.
- `docs/SUGGESTED_COMMIT_PLAN.md` - commit planning.

## Summary

To get started, configure your database, run migrations, and launch `BlindMatchPAS.Web`. The app is ready to use once your connection string is set and the database is created.