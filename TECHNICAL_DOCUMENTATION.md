# BlindMatchPAS Technical Documentation

## Overview

BlindMatchPAS is an ASP.NET Core 8 MVC application that supports academic project proposal submission, blind supervisor review, and controlled match confirmation. It is built as a layered solution with a web application project, unit tests, integration tests, and supporting documentation.

## Repository Structure

- `BlindMatchPAS.sln` - Visual Studio solution file.
- `BlindMatchPAS.Web/` - Main ASP.NET Core MVC application.
- `BlindMatchPAS.UnitTests/` - Unit tests for service and business rule layers.
- `BlindMatchPAS.IntegrationTests/` - Integration tests for data and route protection.
- `docs/` - Additional coursework notes and guidance.

### Main application folders

- `Controllers/` - MVC controllers for web routes and endpoints.
- `Data/` - Entity Framework Core database context, migrations, and seed logic.
- `Models/` - Domain entities, enums, and shared view models.
- `Repositories/` - Data access interfaces and implementations.
- `Services/` - Business logic and application workflow implementations.
- `Utilities/` - Helper extensions used across the application.
- `ViewModels/` - Data transfer objects for views and forms.
- `Views/` - Razor views for the UI.

## Prerequisites

- .NET SDK 8.x
- Visual Studio 2022 or newer (recommended) or another compatible IDE
- SQL Server, SQL Server Express, or LocalDB
- Optional: `dotnet-ef` CLI tool for running migrations

## Setup Instructions

### 1. Open the solution

Open `BlindMatchPAS.sln` in Visual Studio or your preferred C# IDE.

### 2. Configure the database

Update the connection string in:

- `BlindMatchPAS.Web/appsettings.json`
- `BlindMatchPAS.Web/appsettings.Development.json`

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=BlindMatchPASDb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=True"
}
```

Change the server, database, username, and password values for your environment.

### 3. Apply database migrations

The project includes EF Core migrations in `BlindMatchPAS.Web/Data/Migrations`.

Run from Visual Studio Package Manager Console:

```powershell
Update-Database -Project BlindMatchPAS.Web -StartupProject BlindMatchPAS.Web
```

Or run from the .NET CLI:

```bash
dotnet ef database update --project BlindMatchPAS.Web --startup-project BlindMatchPAS.Web
```

### 4. Seed data

Seed data runs automatically on application startup outside the `Testing` environment after migrations are applied. Seeded content includes research areas, demo user accounts, and sample proposals.

## Running the application

### From Visual Studio

1. Set `BlindMatchPAS.Web` as the startup project.
2. Build the solution.
3. Run the application.
4. Log in with a seeded account.

### From the CLI

```bash
dotnet run --project BlindMatchPAS.Web
```

## Default login accounts

All seeded demo accounts use the password: `P@ssword123!`

- System Admin: `admin@blindmatchpas.local`
- Module Leader: `moduleleader@blindmatchpas.local`
- Supervisor 1: `supervisor.ai@blindmatchpas.local`
- Supervisor 2: `supervisor.cyber@blindmatchpas.local`
- Supervisor 3: `supervisor.iot@blindmatchpas.local`
- Student 1: `student.group1@blindmatchpas.local`
- Student 2: `student.group2@blindmatchpas.local`
- Student 3: `student.group3@blindmatchpas.local`

## User roles and flows

### Student

- Submit, edit, and withdraw project proposals.
- Track proposal status and match progress.
- View confirmed supervisor details only after match completion.

### Supervisor

- Browse anonymous proposals by research area.
- Express interest in proposals without seeing student identity.
- Confirm matches and review assigned proposals.

### Admin

- Manage user accounts, research areas, and system settings.
- View audit logs and oversight dashboards.
- Reassign proposals and monitor matching status.

## Running tests

Run the full solution tests:

```bash
dotnet test BlindMatchPAS.sln
```

Run unit tests only:

```bash
dotnet test BlindMatchPAS.UnitTests/BlindMatchPAS.UnitTests.csproj
```

Run integration tests only:

```bash
dotnet test BlindMatchPAS.IntegrationTests/BlindMatchPAS.IntegrationTests.csproj
```

## Development notes

- The application uses ASP.NET Core Identity for authentication and role-based authorization.
- Business logic resides in `Services/` and uses repository patterns from `Repositories/`.
- Database models and relationships are defined in `Models/DomainEntities.cs`.
- Views are implemented with Razor and Bootstrap.
- Audit logging is captured for important operations to support traceability.

## Additional resources

- `ARCHITECTURE_AND_USER_GUIDE.md` - architecture overview and user guide.
- `DEVELOPMENT_NOTES.md` - development-specific implementation notes.
- `docs/SAMPLE_REPORT_SUPPORT_NOTES.md` - report reference notes.
- `docs/SUGGESTED_SCREENSHOTS.md` - suggested screenshot guide.
- `docs/SUGGESTED_COMMIT_PLAN.md` - commit planning advice.

## Summary

This repository is ready to run after configuring the database connection and applying migrations. The main entry point is `BlindMatchPAS.Web`, and testing can be executed through the provided test projects.
