# Blind-Match PAS: Architecture and User Guide

**Blind-Match PAS** (Project Approval System) is an ASP.NET Core 8 MVC web application designed for academic project-supervisor matching with blind review principles. Students submit project proposals that supervisors review anonymously until a confirmed match is made, at which point student identity is revealed.

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Architecture](#architecture)
3. [User Roles and Workflows](#user-roles-and-workflows)
4. [Data Model](#data-model)
5. [Key Features](#key-features)
6. [Security and Business Rules](#security-and-business-rules)
7. [Deployment and Configuration](#deployment-and-configuration)

---

## System Overview

### Purpose

Blind-Match PAS automates the allocation of student projects to academic supervisors through:
- **Anonymous submission and review** — Students submit proposals; supervisors browse and express interest without knowing student details
- **Controlled identity reveal** — Student identification is withheld until a confirmed match is established
- **Role-based dashboards** — Students, supervisors, and admins have tailored interfaces
- **Auditability and oversight** — Admin dashboards track all allocations, expressions of interest, and administrative overrides
- **Research area organization** — Proposals and supervisor expertise are categorized by research domains

---

## Architecture

### Technology Stack

| Component | Technology |
|-----------|-----------|
| Framework | ASP.NET Core 8 MVC |
| Language | C# |
| ORM | Entity Framework Core |
| Database | SQL Server (or SQLite for testing) |
| Authentication | ASP.NET Core Identity |
| UI Framework | Bootstrap 5 with custom premium theme |
| Testing | xUnit, Moq, SQLite/WebApplicationFactory |

### Solution Structure

```
BlindMatchPAS.sln
├── BlindMatchPAS.Web                 (Main MVC application)
│   ├── Controllers/                  (HTTP request handlers)
│   ├── Services/                     (Business logic and workflows)
│   ├── Repositories/                 (Data access abstraction)
│   ├── Models/                       (Domain entities and view models)
│   ├── Data/                         (EF Core context and migrations)
│   ├── Views/                        (Razor templates)
│   └── Utilities/                    (Extension methods and helpers)
├── BlindMatchPAS.UnitTests           (Service and business rule tests)
├── BlindMatchPAS.IntegrationTests    (Route protection and repository tests)
└── docs/                             (Supporting documentation)
```

### High-Level Component Diagram

The application follows a classic **layered architecture pattern** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                     Browser / User                           │
│   (Student, Supervisor, Admin roles via MVC views)          │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTP Request
                     │
┌────────────────────▼────────────────────────────────────────┐
│       *** PRESENTATION LAYER ***                            │
│         ASP.NET Core MVC (Controllers)                      │
│  • StudentController      — Handles /student/* routes       │
│  • SupervisorController   — Handles /supervisor/* routes    │
│  • AdminController        — Handles /admin/* routes         │
│  • AccountController      — Login/Registration              │
│                                                              │
│  Role:                                                       │
│  - Route HTTP requests to appropriate service calls         │
│  - Extract user identity (via User.GetUserId())             │
│  - Validate anti-forgery tokens on POST/PUT/DELETE          │
│  - Check authorization attributes (@Authorize, roles)       │
│  - Return appropriate view or status code                   │
└────────────────────┬────────────────────────────────────────┘
                     │ Controller calls Service methods
                     │
┌────────────────────▼────────────────────────────────────────┐
│       *** BUSINESS LOGIC LAYER ***                          │
│         Services (Interfaces in ServiceContracts)           │
│  • ProposalService                                          │
│    └─ Create/Edit/Withdraw proposals                        │
│    └─ Validate proposal state transitions                   │
│    └─ Build view models for students                        │
│                                                              │
│  • MatchingService                                          │
│    └─ Browse proposals anonymously                          │
│    └─ Express interest (with duplicate prevention)          │
│    └─ Confirm matches (reveals identity)                    │
│    └─ Manage supervisor expertise                           │
│                                                              │
│  • AdminService                                             │
│    └─ User and research area management                    │
│    └─ Proposal and match oversight                          │
│    └─ Manual reassignment workflows                         │
│    └─ System statistics & dashboards                        │
│                                                              │
│  • AuditService                                             │
│    └─ Log all business-critical actions                     │
│    └─ Record action, entity, user, timestamp, details       │
│                                                              │
│  Role:                                                       │
│  - Enforce business rules (e.g., no editing matched         │
│    proposals, prevent duplicate interest)                   │
│  - Orchestrate workflow logic (proposal lifecycle)          │
│  - Call repositories for data access                        │
│  - Call audit service for logging                           │
│  - Return ServiceResult<T> (success/error with messages)    │
│  - Build domain-specific view models                        │
└────────────────────┬────────────────────────────────────────┘
                     │ Service calls Repository methods
                     │
┌────────────────────▼────────────────────────────────────────┐
│       *** DATA ACCESS LAYER ***                             │
│         Repository Interfaces & Implementations             │
│  • IProposalRepository / ProposalRepository                 │
│    └─ CRUD on Proposal, SupervisorInterest, Match           │
│    └─ Query by student, research area, status               │
│    └─ Check interest/match existence                        │
│                                                              │
│  • IResearchAreaRepository / ResearchAreaRepository         │
│    └─ Fetch active/inactive research areas                  │
│    └─ Query area by ID or name                              │
│    └─ Supervisor expertise lookup                           │
│                                                              │
│  • IAuditLogRepository / AuditLogRepository                 │
│    └─ Insert audit log entries                              │
│    └─ Query logs by filter (user, action, entity)           │
│                                                              │
│  Role:                                                       │
│  - Abstract database access behind interface contracts      │
│  - Encapsulate EF Core queries                              │
│  - Enable unit testing via mock repositories                │
│  - Provide single point of change for data access logic     │
│  - Return entities (not view models)                        │
└────────────────────┬────────────────────────────────────────┘
                     │ Repository uses DbContext & LINQ
                     │
┌────────────────────▼────────────────────────────────────────┐
│       *** DATA PERSISTENCE LAYER ***                        │
│      Entity Framework Core + SQL Database                   │
│  • ApplicationDbContext                                     │
│    └─ DbSet<Proposal>, DbSet<ApplicationUser>, etc.         │
│    └─ Fluent model configuration                            │
│    └─ Relationship mappings                                 │
│                                                              │
│  • Entity Classes (DomainEntities.cs)                       │
│    └─ Proposal, Match, ApplicationUser, StudentProfile      │
│    └─ SupervisorProfile, ResearchArea, AuditLog, etc.       │
│                                                              │
│  • Migrations (Migrations/ folder)                          │
│    └─ Database schema versioning                            │
│    └─ Deploy database structure changes                     │
│    └─ Reversible with Down() methods                        │
│                                                              │
│  • SQL Server Database                                      │
│    └─ Stores all application data                           │
│    └─ Enforces unique constraints (proposal, email)         │
│    └─ Defines foreign key relationships                     │
│                                                              │
│  • SQLite Database (Testing)                                │
│    └─ In-memory or file-based for tests                     │
│    └─ Matches SQL Server schema via migrations              │
│    └─ Enables fast test execution without server            │
│                                                              │
│  Role:                                                       │
│  - Map C# objects to SQL tables                             │
│  - Generate SQL queries from LINQ expressions               │
│  - Manage migrations and schema versions                    │
│  - Persist and retrieve entity data                         │
└─────────────────────────────────────────────────────────────┘
```

### Data Flow Example: Student Creates a Proposal

```
1. User clicks "Create Proposal" button in browser
   ↓
2. Browser sends GET /student/proposals/create
   ↓
3. StudentController.CreateProposal() receives request
   └─ Validates [Authorize(Roles = "Student")]
   └─ Extracts userId = User.GetUserId()
   ↓
4. Controller calls proposalService.BuildProposalFormAsync(userId)
   └─ Service queries db for research areas
   └─ Service queries db for student's previous proposals
   └─ Service builds ProposalFormViewModel with dropdown data
   ↓
5. Service calls repository.GetActiveResearchAreasAsync()
   └─ Repository uses EF Core to query ApplicationDbContext
   └─ EF Core translates LINQ to SQL:
      SELECT * FROM ResearchAreas WHERE IsActive = 1
   ↓
6. Database returns active areas; repository returns to service
   ↓
7. Service builds view model and returns to controller
   ↓
8. Controller renders ProposalForm.cshtml with form data
   ↓
9. User sees form with research area dropdown and enters details
   ↓
10. User submits POST /student/proposals/create

11. StudentController.CreateProposal(model) receives form data
    └─ Validates [ValidateAntiForgeryToken] (CSRF protection)
    └─ Validates ModelState (required fields, formats)
    ↓
12. Controller calls proposalService.CreateProposalAsync(userId, model)
    └─ Service enforces business rule:
       "Research area must be active"
    └─ Service creates Proposal entity with status = Draft
    └─ Service calls repository.CreateProposalAsync(proposal)
    ↓
13. Repository calls dbContext.Proposals.AddAsync(proposal)
    └─ Adds entity to change tracking
    └─ Marks as "Added"
    └─ Calls dbContext.SaveChangesAsync()
    └─ EF Core generates INSERT SQL:
       INSERT INTO Proposals (StudentUserId, ResearchAreaId, ...)
       VALUES (@studentId, @areaId, ...)
    ↓
14. Database inserts row and returns auto-generated ProposalId
    ↓
15. EF Core updates proposal.Id with database-generated value
    ↓
16. Service calls auditService.RecordAsync()
    └─ Audit service logs: "Proposal Created" with proposal ID
    └─ Audit repository inserts audit log to database
    ↓
17. Service returns ServiceResult<int>{Data = proposalId, Success = true}
    ↓
18. Controller checks result.Success
    └─ If true: Redirects to /student/proposals (success message)
    └─ If false: Redisplays form with error message
    ↓
19. Browser displays success message and refreshes proposals list
```

### Service Layer Overview

| Service | Purpose | Key Methods |
|---------|---------|-------------|
| **ProposalService** | Student proposal lifecycle management | `CreateProposalAsync`, `UpdateProposalAsync`, `WithdrawProposalAsync` |
| **MatchingService** | Supervisor browsing, interest, and match confirmation | `ExpressInterestAsync`, `ConfirmMatchAsync`, `GetAnonymousBrowserAsync` |
| **AdminService** | Admin dashboards, user management, research areas, reassignment | `SaveResearchAreaAsync`, `CreateUserAsync`, `ReassignAsync` |
| **AuditService** | Records business-critical actions for traceability | `RecordAsync` |

---

## User Roles and Workflows

### 1. Student Workflow

**Role:** `Student`

**Permissions:**
- Create, edit, and withdraw proposals (before match)
- View own proposals and their statuses
- Access student dashboard with proposal summary

**Workflow:**

```
1. Create Proposal
   ├─ Select research area
   ├─ Enter proposal details (title, description, keywords)
   ├─ Save as Draft or Submit
   └─ Proposal enters PendingReview status

2. Monitor Proposal
   ├─ View proposal status on dashboard
   ├─ Edit proposal (if not yet submitted or only in Draft)
   ├─ Withdraw proposal (unless already matched)
   └─ Track supervisor interest (see count after match)

3. Upon Match Confirmation
   ├─ Identity revealed to supervisor
   ├─ Cannot edit or withdraw proposal anymore
   └─ Match details available in dashboard
```

**Views:**
- **Student Dashboard** — Overview of proposals, statuses, and matched supervisors
- **Proposals List** — All proposals with lifecycle status
- **Proposal Form** — Create or edit proposal
- **Proposal Details** — View full proposal information

---

### 2. Supervisor Workflow

**Role:** `Supervisor`

**Permissions:**
- Browse proposals anonymously (blind view)
- Express interest in proposals
- Confirm matches
- Manage expertise areas
- View interested and matched proposals

**Workflow:**

```
1. Configure Expertise
   ├─ Set department and specialization
   ├─ Add expertise areas matching research categories
   └─ Update expertise as needed

2. Anonymous Browsing
   ├─ Filter proposals by research area or search
   ├─ View proposal details WITHOUT student identity
   ├─ Assess proposal fit against expertise
   └─ No student contact info visible

3. Express Interest
   ├─ Select proposal and click "Express Interest"
   ├─ Interest recorded and audited
   ├─ Cannot express duplicate interest
   └─ Interest visible in "Interested" tab

4. Confirm Match
   ├─ From "Interested" proposals, confirm match
   ├─ System creates Match record
   ├─ Proposal status changes to Matched
   ├─ Student identity revealed to supervisor
   ├─ Cannot have multiple matches for same proposal
   └─ View matched proposals in "My Matches" tab
```

**Views:**
- **Supervisor Dashboard** — Statistics and quick links
- **Anonymous Browser** — Blind proposal search and filtering
- **Interested Proposals** — Proposals with expressed interest
- **Confirmed Matches** — Matched proposals with revealed student details
- **Expertise Management** — Edit supervisor expertise areas

---

### 3. Admin Workflow

**Role:** `Admin`

**Permissions:**
- Full system oversight and management
- Create and activate/deactivate users
- Manage research areas
- View all proposals and matches
- Perform manual reassignments (with audit trail)
- Access audit logs

**Workflow:**

```
1. User Management
   ├─ Create new users (Student, Supervisor, or Admin)
   ├─ Set initial passwords
   ├─ Activate/deactivate user accounts
   └─ All actions audited

2. Research Area Management
   ├─ Create new research areas
   ├─ Edit area names and descriptions
   ├─ Set visual accent colors
   ├─ Activate/deactivate areas
   └─ Controls proposal categorization

3. Proposal Oversight
   ├─ Search proposals by student, title, or keyword
   ├─ Filter by research area and status
   ├─ View complete proposal details with student identity
   ├─ Track proposal lifecycle
   └─ Identify unmatched proposals

4. Match Oversight
   ├─ View all confirmed matches
   ├─ Track match timestamps and supervisors
   ├─ Identify proposals awaiting confirmation

5. Manual Reassignment (Override)
   ├─ Access proposal and select new supervisor
   ├─ System validates supervisor expertise and match legality
   ├─ Creates reassignment record with MatchStatus = Reassigned
   ├─ Records reason and acting admin in audit log
   └─ Used for exceptional cases (supervisor withdrawal, etc.)

6. Audit Log Review
   ├─ View all recorded actions
   ├─ Filter by action type, entity, or user
   ├─ Verify system compliance and traceability
   └─ Export logs if needed
```

**Views:**
- **Admin Dashboard** — System statistics and quick stats
- **User Management** — Create and manage users
- **Research Area Management** — Edit research categories
- **Proposal Oversight** — Search and inspect all proposals
- **Match Oversight** — View all confirmed matches
- **Reassignment Interface** — Select new supervisor and override match
- **Audit Logs** — View and filter recorded actions

---

## Data Model

### Core Entities

#### ApplicationUser
Extends ASP.NET Core Identity user with academic metadata.

```
ApplicationUser
├── Id (PK, string)
├── DisplayName (string)
├── RegistrationNumber (string, optional)
├── IsActive (bool)
├── Email, PasswordHash, etc. (Identity)
│
├── StudentProfile (0..1)
├── SupervisorProfile (0..1)
├── OwnedProposals (1..*)
├── SupervisorInterests (0..*)
├── SupervisorMatches (0..*)
└── AuditLogs (0..*)
```

#### StudentProfile
Stores student-specific academic information.

```
StudentProfile
├── Id (PK)
├── UserId (FK, ApplicationUser)
├── StudentIdentifier (string, unique)
├── Programme (string)
├── GroupName (string)
├── TeamMemberNames (string)
└── User (reference)
```

#### SupervisorProfile
Stores supervisor expertise and organizational details.

```
SupervisorProfile
├── Id (PK)
├── UserId (FK, ApplicationUser)
├── Department (string)
├── Specialization (string)
├── OfficeLocation (string)
├── ExpertiseAreas (0..*)
└── User (reference)
```

#### Proposal
Core entity representing a student project submission.

```
Proposal
├── Id (PK)
├── StudentUserId (FK, ApplicationUser)
├── ResearchAreaId (FK, ResearchArea)
├── Title (string)
├── Description (string)
├── Status (enum: Draft, Submitted, PendingReview, UnderReview, Matched, Withdrawn)
├── SubmittedAt (DateTime, nullable)
├── CreatedAt (DateTime)
├── Keywords (0..*)
├── SupervisorInterests (0..*)
├── Match (0..1)
├── StatusHistory (0..*)
└── Student, ResearchArea (references)
```

**ProposalStatus States:**

| State | Trigger | Next States | Rules |
|-------|---------|------------|-------|
| **Draft** | Created but not submitted | Submitted, Withdrawn | Student can edit |
| **Submitted** | Student clicks Submit | PendingReview, Withdrawn | Awaiting admin review |
| **PendingReview** | Admin marks for review | UnderReview, Withdrawn | Under admin review |
| **UnderReview** | Admin activates review | Matched, Withdrawn | Supervisors can browse |
| **Matched** | Supervisor confirms match | — | Student identity revealed; proposal locked |
| **Withdrawn** | Student or admin action | — | Terminal state; no further changes |

#### ResearchArea
Categorizes proposals and supervisor expertise.

```
ResearchArea
├── Id (PK)
├── Name (string, unique)
├── Description (string)
├── AccentColor (string, hex)
├── IsActive (bool)
├── Proposals (0..*)
└── SupervisorExpertise (0..*)
```

#### SupervisorExpertise
Links supervisors to research areas they can supervise.

```
SupervisorExpertise
├── Id (PK)
├── SupervisorProfileId (FK, SupervisorProfile)
├── ResearchAreaId (FK, ResearchArea)
├── SupervisorProfile, ResearchArea (references)
```

#### SupervisorInterest
Records supervisor interest in proposals (before matching).

```
SupervisorInterest
├── Id (PK)
├── ProposalId (FK, Proposal)
├── SupervisorUserId (FK, ApplicationUser)
├── ExpressedAt (DateTime)
├── Proposal, Supervisor (references)
```

**Business Rule:** A supervisor cannot express duplicate interest in the same proposal.

#### Match
Confirmed allocation of a proposal to a supervisor.

```
Match
├── Id (PK)
├── ProposalId (FK, Proposal, unique)
├── SupervisorUserId (FK, ApplicationUser)
├── Status (enum: Confirmed, Reassigned)
├── CreatedAt (DateTime)
├── IdentityRevealedAt (DateTime)
└── Proposal, Supervisor (references)
```

**MatchStatus:**
- **Confirmed** — Normal supervisor confirmation flow
- **Reassigned** — Admin manual override/reassignment

#### ProposalStatusHistory
Audit trail of proposal lifecycle transitions.

```
ProposalStatusHistory
├── Id (PK)
├── ProposalId (FK, Proposal)
├── OldStatus (enum)
├── NewStatus (enum)
├── ChangedAt (DateTime)
├── ChangedByUserId (FK, ApplicationUser, nullable)
└── Proposal (reference)
```

#### AuditLog
System-wide audit trail for compliance and traceability.

```
AuditLog
├── Id (PK)
├── Action (string)
├── EntityName (string)
├── EntityId (string)
├── UserId (FK, ApplicationUser)
├── Details (string)
├── Timestamp (DateTime)
└── User (reference)
```

**Audited Actions:**
- User creation/updates
- Proposal creation/submission/withdrawal
- Interest expressions
- Match confirmations
- Admin reassignments
- Research area changes

---

## Key Features

### 1. Blind Review Mechanism

**How it works:**

1. **Anonymous Browsing** — Supervisors view proposals in an anonymous browser without seeing student names, email, profile, or identifying info.
2. **Proposal Details (Blind)** — Shows proposal title, description, keywords, and research area but not student identity.
3. **Interest Expression** — Supervisors express interest without revealing themselves (supervisors know each other; the system tracks who's interested).
4. **Match Confirmation** — When a supervisor confirms interest as a match, the system:
   - Creates a `Match` record
   - Updates proposal status to `Matched`
   - Records the identity reveal timestamp
   - Student identity becomes visible to supervisor in subsequent views

**Security:**
- Student email, registration number, profile details excluded from blind views
- View-level checks prevent accidental identity leakage
- Tests verify blind view behavior

### 2. Proposal Lifecycle Management

Students progress proposals through defined states:

```
Draft → Submitted → PendingReview → UnderReview → Matched
                  ↓          ↓            ↓          ↓
            (withdrawn)  (withdrawn) (withdrawn)  (locked)
```

**Key Rules:**
- Only **Draft** or initial **Submitted** proposals can be edited
- **Matched** proposals cannot be edited or withdrawn
- **Withdrawn** is terminal—no reversions

### 3. Research Area Categorization

- Proposals must belong to a research area
- Supervisors choose expertise areas
- Blind browser can filter by area
- Admin can activate/deactivate areas (affects proposal views and categorization)

### 4. Multi-User Roles with RBAC

Three distinct user types with role-based access control:

```
ASP.NET Core Identity Roles:
├── Student
├── Supervisor
└── Admin
```

Each role has:
- Dedicated controllers and routes
- Role-based `[Authorize(Roles = "...")]` attributes
- Separate dashboard and feature access
- Custom view models for role-specific data

### 5. Audit Logging and Compliance

Critical actions are logged:
- User creation and status changes
- Proposal creation, submission, withdrawal
- Interest expressions and match confirmations
- Admin reassignments
- Research area modifications

Admins can query audit logs by:
- Action type
- Entity (User, Proposal, Match, etc.)
- User performing action
- Timestamp range

### 6. Admin Reassignment (Override)

Handles exceptions (e.g., supervisor withdrawal):

1. Admin selects proposal and new supervisor
2. System validates:
   - Supervisor exists and is active
   - Supervisor has expertise in proposal's research area
   - No existing confirmed match (unless overriding)
3. Reassignment creates new `Match` with `MatchStatus = Reassigned`
4. Old interest/match records archived (soft delete via status)
5. Action recorded in audit log with admin identity and reason

---

## Security and Business Rules

### Identity Protection Rules

| Context | Visible | Hidden |
|---------|---------|--------|
| **Anonymous Browser** (Supervisor) | Title, Description, Keywords, Research Area, SupervisorInterests Count | Student Name, Email, ID |
| **Interested View** (Supervisor) | Title, Status, Interest Count | Student Name, Email |
| **Post-Match View** (Supervisor) | Full Proposal + Student Profile | — |
| **Student Dashboard** | Own Proposals, Matched Supervisors | Supervisors until matched |

### Business Rules Enforcement

**Proposal Editing:**
```csharp
if (proposal.Status == ProposalStatus.Matched)
    throw new InvalidOperationException("Matched proposals cannot be edited");
```

**Withdrawal Prevention:**
```csharp
if (proposal.Status == ProposalStatus.Matched)
    throw new InvalidOperationException("Matched proposals cannot be withdrawn");
```

**Duplicate Interest Prevention:**
```csharp
var existingInterest = await _proposalRepository.GetInterestAsync(proposalId, supervisorId);
if (existingInterest != null)
    throw new InvalidOperationException("Interest already expressed");
```

**Double-Match Prevention:**
```csharp
var existingMatch = await _proposalRepository.GetMatchAsync(proposalId);
if (existingMatch != null && existingMatch.Status == MatchStatus.Confirmed)
    throw new InvalidOperationException("Proposal already matched");
```

**Inactive Research Area:**
```csharp
if (!researchArea.IsActive)
    throw new InvalidOperationException("Cannot create proposal in inactive research area");
```

### Authentication & Authorization

- **ASP.NET Core Identity** handles login, password encryption, and cookie-based sessions
- **Password Policy:** Min 8 characters, uppercase, lowercase, digit, non-alphanumeric
- **Unique Email Enforcement** — Enforced by Identity and database unique constraint
- **Role-Based Access Control** — `[Authorize(Roles = "Student")]`, etc.
- **Anti-Forgery Tokens** — All POST/PUT/DELETE actions validate tokens
- **Ownership Checks** — Students can only edit/withdraw own proposals

---

## Deployment and Configuration

### Prerequisites

- Visual Studio 2022 or later (with .NET 8 workload)
- .NET SDK 8.x
- SQL Server or SQL Server Express / LocalDB
- Optional: EF Core tools (`dotnet-ef` CLI)

### Configuration

**Connection String** (appsettings.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BlindMatchPAS;Trusted_Connection=true;"
  },
  "DatabaseProvider": "SqlServer"
}
```

**For SQLite (testing):**
```json
{
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=blindmatchpas.db"
  },
  "DatabaseProvider": "Sqlite"
}
```

### Database Setup

1. **Create Database:**
   ```bash
   Update-Database
   ```
   (or via `dotnet ef` CLI)

2. **Seed Demo Data:**
   - `DatabaseSeeder` automatically runs on application startup (unless in "Testing" environment)
   - Creates demo users (Student, Supervisor, Admin) and sample proposals
   - Seeded credentials available in source code

### Running the Application

1. Open `BlindMatchPAS.sln` in Visual Studio
2. Set `BlindMatchPAS.Web` as startup project
3. Press **F5** or Build → Run

Application starts at `https://localhost:7000` (adjust port in `launchSettings.json` if needed)

### Testing

**Unit Tests:**
```bash
dotnet test BlindMatchPAS.UnitTests
```

Covers:
- Service business rules (matched proposal edit prevention, duplicate interest prevention, etc.)
- Research area activation/deactivation
- Admin reassignment validation

**Integration Tests:**
```bash
dotnet test BlindMatchPAS.IntegrationTests
```

Covers:
- Repository functionality
- Route protection (unauthorized access prevention)
- Database integration via real SQLite instance

---

## Summary

Blind-Match PAS provides a robust, secure platform for academic project supervision allocation with:

✓ Anonymous proposal review until confirmed match  
✓ Role-based workflows for students, supervisors, and admins  
✓ Comprehensive audit logging  
✓ Business rule enforcement (no editing matched proposals, duplicate interest prevention, etc.)  
✓ Clean architectural layers (Controllers → Services → Repositories → Database)  
✓ Strong security practices (RBAC, anti-forgery, identity protection)  
✓ Test coverage for critical business logic  

---

## Quick Reference: API and View Routes

### Student Routes
- `GET /student/dashboard` — Dashboard
- `GET /student/proposals` — Proposals list
- `GET /student/proposals/create` — Create form
- `POST /student/proposals/create` — Create submission
- `GET /student/proposals/{id}/edit` — Edit form
- `POST /student/proposals/{id}/edit` — Update submission
- `POST /student/proposals/{id}/withdraw` — Withdraw proposal

### Supervisor Routes
- `GET /supervisor/dashboard` — Dashboard
- `GET /supervisor/proposals/browse` — Anonymous browser
- `POST /supervisor/proposals/{id}/interest` — Express interest
- `GET /supervisor/proposals/interested` — Interested proposals
- `GET /supervisor/proposals/matches` — Confirmed matches
- `POST /supervisor/proposals/{id}/confirm-match` — Confirm match
- `GET /supervisor/expertise` — Expertise management
- `POST /supervisor/expertise` — Update expertise

### Admin Routes
- `GET /admin/dashboard` — Dashboard
- `GET /admin/users` — User management
- `POST /admin/users` — Create user
- `GET /admin/research-areas` — Research area management
- `POST /admin/research-areas` — Save research area
- `GET /admin/proposals` — Proposal oversight
- `GET /admin/matches` — Match oversight
- `GET /admin/proposals/{id}/reassign` — Reassignment interface
- `POST /admin/proposals/{id}/reassign` — Perform reassignment
- `GET /admin/audit-logs` — View audit logs

---

**Document Version:** 1.0  
**Last Updated:** April 2026  
**Audience:** Developers, System Administrators, Project Stakeholders
