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

The services layer implements all business logic, orchestrates workflows, and enforces critical rules. Services are registered as scoped dependencies in the DI container and are always injected into controllers.

#### Service Architecture Pattern

All services follow a consistent pattern:

```csharp
public interface IXxxService
{
    Task<ServiceResult<T>> OperationAsync(parameters);
    Task<ServiceResult> ActionAsync(parameters);
}

public class XxxService : IXxxService
{
    private readonly IRepository _repository;
    private readonly IAuditService _auditService;
    
    public XxxService(IRepository repository, IAuditService auditService)
    {
        _repository = repository;
        _auditService = auditService;
    }
    
    public async Task<ServiceResult<T>> OperationAsync(parameters)
    {
        // Validate inputs
        // Enforce business rules
        // Perform operations
        // Record audit logs
        // Return ServiceResult with success/error
    }
}
```

**ServiceResult Pattern:**
```csharp
public class ServiceResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }
}
```

This pattern allows controllers to handle success/failure uniformly without exceptions.

---

#### 1. ProposalService

**Purpose:** Manages the complete student proposal lifecycle from creation through withdrawal or matching.

**Dependencies:**
- `IProposalRepository` — Proposal data access
- `IAuditService` — Audit logging
- `IResearchAreaRepository` — Validate research areas

**Key Methods:**

| Method | Parameters | Returns | Business Logic |
|--------|-----------|---------|-----------------|
| `GetStudentDashboardAsync(studentUserId)` | Student ID | `StudentDashboardViewModel` | Fetches all proposals for student; counts by status; calculates matching stats |
| `GetStudentProposalsAsync(studentUserId)` | Student ID | `List<ProposalListItemViewModel>` | Retrieves proposals filtered by student; orders by creation date |
| `BuildProposalFormAsync(studentUserId, proposalId?)` | Student ID, optional proposal ID | `ProposalFormViewModel` | Loads research areas dropdown; populates existing proposal if editing |
| `CreateProposalAsync(studentUserId, model)` | Student ID, form model | `ServiceResult<int>` | **Validates:** Research area is active; prevents null fields. **Creates:** Proposal with Draft status; records audit log. **Returns:** New proposal ID or error |
| `UpdateProposalAsync(studentUserId, proposalId, model)` | Student ID, proposal ID, form model | `ServiceResult` | **Validates:** Student owns proposal; status is Draft/Submitted (not Matched/Withdrawn). **Prevents:** Editing locked proposals. **Updates:** Proposal properties; records audit log |
| `WithdrawProposalAsync(studentUserId, proposalId)` | Student ID, proposal ID | `ServiceResult` | **Validates:** Student owns proposal; status NOT Matched. **Prevents:** Withdrawing matched proposals. **Updates:** Status to Withdrawn; timestamps; records audit |
| `GetStudentProposalDetailsAsync(studentUserId, proposalId)` | Student ID, proposal ID | `ProposalDetailsViewModel?` | Fetches proposal with full details (keywords, supervisor interests count if matched) |

**Critical Business Rules:**

- ✓ Only active research areas can be selected for proposals
- ✓ Proposals start in **Draft** status (editable by creator)
- ✓ **Matched** proposals are locked (no edit, no withdrawal)
- ✓ Only the owning student can modify their proposals
- ✓ Withdrawal is allowed unless proposal is already matched
- ✓ Submit action transitions proposal to `PendingReview` (awaiting admin activation)

**Audit Events Recorded:**
- "Proposal Created" — When proposal is first created
- "Proposal Updated" — When fields are modified
- "Proposal Withdrawn" — When student withdraws

---

#### 2. MatchingService

**Purpose:** Manages supervisor proposal review, interest expression, and match confirmation. Implements the blind review workflow.

**Dependencies:**
- `IProposalRepository` — Proposal and interest data access
- `IResearchAreaRepository` — Expertise validation
- `IAuditService` — Audit logging

**Key Methods:**

| Method | Parameters | Returns | Business Logic |
|--------|-----------|---------|-----------------|
| `GetSupervisorDashboardAsync(supervisorUserId)` | Supervisor ID | `SupervisorDashboardViewModel` | Counts interested proposals; counts confirmed matches; loads quick stats |
| `GetAnonymousBrowserAsync(researchAreaId?, searchTerm?)` | Optional area ID, optional search | `AnonymousProposalBrowserViewModel` | Filters UnderReview proposals by area/search; **excludes student identity**; shows only title, description, keywords, interest count |
| `GetSupervisorProposalDetailsAsync(supervisorUserId, proposalId)` | Supervisor ID, proposal ID | `SupervisorProposalDetailsViewModel?` | If proposal UnderReview: **blind view** (no student name/email). If proposal Matched to this supervisor: **full view** including student profile. Otherwise: null |
| `GetInterestedAsync(supervisorUserId)` | Supervisor ID | `List<SupervisorProposalCardViewModel>` | Fetches proposals where supervisor expressed interest but not yet confirmed match |
| `GetConfirmedMatchesAsync(supervisorUserId)` | Supervisor ID | `List<SupervisorProposalCardViewModel>` | Fetches proposals where supervisor has confirmed match; includes student identity |
| `BuildExpertiseViewModelAsync(supervisorUserId)` | Supervisor ID | `ExpertiseManagementViewModel` | Loads supervisor profile; loads all research areas as checkboxes; marks currently-selected expertise |
| `UpdateExpertiseAsync(supervisorUserId, model)` | Supervisor ID, expertise model | `ServiceResult` | **Updates:** SupervisorExpertise records; **records audit log** |
| `ExpressInterestAsync(supervisorUserId, proposalId)` | Supervisor ID, proposal ID | `ServiceResult` | **Validates:** Proposal is UnderReview; supervisor not already interested (prevents duplicate); creates SupervisorInterest record; records audit |
| `ConfirmMatchAsync(supervisorUserId, proposalId)` | Supervisor ID, proposal ID | `ServiceResult` | **Validates:** Supervisor expressed interest in proposal; no existing confirmed match on proposal. **Creates:** Match record with Status=Confirmed; updates Proposal.Status to Matched; sets IdentityRevealedAt timestamp; **records audit** |

**Critical Business Rules:**

- ✓ Supervisors can only browse proposals in **UnderReview** status
- ✓ Student identity is **never visible** in blind browser (title, description only)
- ✓ Duplicate interest prevention — Supervisor cannot express interest in same proposal twice
- ✓ One match per proposal — Only one confirmed match allowed per proposal
- ✓ Interest precedes match — Supervisor must express interest before confirming match
- ✓ Identity reveal on match — Student name/email shown only after match confirmed
- ✓ Expertise optional but validated — Supervisors can operate without expertise, but reassignment validates expertise fit

**Identity Protection Mechanisms:**

1. **Anonymous Browser Query:**
   ```csharp
   // SELECT only: Title, Description, Keywords, ResearchArea, InterestCount
   // EXCLUDE: StudentName, StudentEmail, StudentProfile, Registration#
   var proposals = await _repo.GetProposalsForBrowsingAsync();
   ```

2. **View-Level Filtering:**
   - Controllers check Match status before returning student profile
   - Razor views conditionally render student details only if matched

3. **Tests Verify:**
   - Blind views don't contain student PII
   - Post-match views include student details

**Audit Events Recorded:**
- "Interest Expressed" — When supervisor shows interest
- "Match Confirmed" — When supervisor confirms match
- "Expertise Updated" — When supervisor modifies expertise areas

---

#### 3. AdminService

**Purpose:** Provides system oversight, user management, research area administration, and manual reassignment capabilities. Admins have full visibility (no blind views).

**Dependencies:**
- `IProposalRepository` — Proposal oversight
- `IResearchAreaRepository` — Area management
- `IAuditLogRepository` — Audit log access
- `UserManager<ApplicationUser>` — Identity user management (ASP.NET Core Identity)
- `IAuditService` — Audit logging

**Key Methods:**

| Method | Parameters | Returns | Business Logic |
|--------|-----------|---------|-----------------|
| `GetDashboardAsync()` | — | `AdminDashboardViewModel` | Computes system statistics: total proposals by status, total matches, total users by role, pending approvals |
| `GetResearchAreasAsync()` | — | `ResearchAreaManagementViewModel` | Fetches all research areas (active and inactive) with counts of proposals in each |
| `SaveResearchAreaAsync(model)` | Research area edit model | `ServiceResult` | **Creates or Updates:** ResearchArea; validates name uniqueness; records audit |
| `ToggleResearchAreaAsync(areaId)` | Research area ID | `ServiceResult` | **Toggles:** IsActive flag; if deactivating, may prevent new proposals in that area; records audit |
| `GetUsersAsync()` | — | `UserManagementViewModel` | Fetches all users with roles and active status |
| `CreateUserAsync(model)` | User creation model | `ServiceResult` | **Validates:** Email unique; password meets policy. **Creates:** ApplicationUser via Identity; assigns role. **Initializes:** Profile (StudentProfile or SupervisorProfile based on role). **Records audit** |
| `ToggleUserAsync(userId)` | User ID | `ServiceResult` | **Updates:** User.IsActive flag; records audit |
| `GetProposalOversightAsync(searchTerm?, areaId?, status?)` | Optional search, area, status | `ProposalOversightViewModel` | **Queries all proposals** with filters; shows full student identity (unlike supervisor browse); includes proposal status, created date, supervisor (if matched) |
| `GetMatchOversightAsync()` | — | `MatchOversightViewModel` | Fetches all confirmed matches; shows student, supervisor, match timestamp |
| `BuildReassignmentViewModelAsync(proposalId)` | Proposal ID | `AdminReassignmentViewModel?` | Loads proposal; lists all active supervisors; pre-populates current supervisor if any; validates proposal is matchable |
| `ReassignAsync(model, actingUserId)` | Reassignment model, admin user ID | `ServiceResult` | **Validates:** New supervisor is active; supervisor has expertise in proposal's area. **Creates:** New Match with Status=Reassigned (overrides old match). **Records audit** with details: reason, old supervisor, new supervisor, acting admin |
| `GetAuditLogsAsync()` | — | `List<AuditLogViewModel>` | Fetches all audit log entries; typically paginated; can be filtered by date range, action, entity, user |

**Critical Business Rules:**

- ✓ Full visibility — Admins see student identity in all views (no blind browsing)
- ✓ User creation enforces password policy — 8+ chars, uppercase, lowercase, digit, non-alphanumeric
- ✓ Email uniqueness — Enforced by Identity and database
- ✓ Research area activation/deactivation — Controls what fields are available for proposals
- ✓ Reassignment expertise validation — New supervisor must have expertise in proposal's research area
- ✓ Reassignment creates audit trail — Reason, old/new supervisors, timestamp, acting admin logged

**Admin Reassignment Workflow:**

1. Admin selects proposal
2. Admin chooses new supervisor from active supervisor list
3. System validates:
   - New supervisor is active
   - New supervisor has expertise in proposal's research area
   - Proposal is not in terminal state (withdrawn)
4. System creates new **Match** with `Status = Reassigned`
5. Old Match record archived/marked as superseded
6. Proposal status updated (if needed)
7. Audit log entry created with full context

**Audit Events Recorded:**
- "User Created" — When admin creates new account
- "User Deactivated/Activated" — When toggling user status
- "Research Area Saved" — When area created/modified
- "Research Area Toggled" — When area activated/deactivated
- "Proposal Reassigned" — When admin manually reassigns supervisor

---

#### 4. AuditService

**Purpose:** Records all business-critical actions for compliance, traceability, and forensic analysis.

**Dependencies:**
- `IAuditLogRepository` — Audit log data access

**Key Methods:**

| Method | Parameters | Returns | Business Logic |
|--------|-----------|---------|-----------------|
| `RecordAsync(action, entityName, entityId, userId, details)` | Action string, entity name, entity ID, user ID (nullable), details string | `Task` (fire-and-forget, no return) | **Inserts:** AuditLog record with action, entity reference, user, timestamp, and context details |

**Usage Pattern:**

```csharp
// In a service:
await _auditService.RecordAsync(
    action: "Proposal Created",
    entityName: "Proposal",
    entityId: proposal.Id.ToString(),
    userId: studentUserId,
    details: $"Title: {proposal.Title}, ResearchArea: {proposal.ResearchArea.Name}"
);

// In admin service:
await _auditService.RecordAsync(
    action: "Proposal Reassigned",
    entityName: "Match",
    entityId: match.Id.ToString(),
    userId: adminUserId,
    details: $"From: {oldSupervisor.DisplayName} → To: {newSupervisor.DisplayName}, Reason: {reason}"
);
```

**Logged Actions:**

| Action | Entity | When | Details Include |
|--------|--------|------|-----------------|
| Proposal Created | Proposal | Student submits | Title, Research Area |
| Proposal Updated | Proposal | Student edits | Changed fields |
| Proposal Withdrawn | Proposal | Student/Admin withdraws | Reason if admin |
| Interest Expressed | SupervisorInterest | Supervisor clicks "Interest" | Supervisor, Proposal |
| Match Confirmed | Match | Supervisor confirms | Supervisor, Proposal |
| Proposal Reassigned | Match | Admin reassigns | Old/New Supervisor, Reason |
| User Created | ApplicationUser | Admin creates account | Email, Role, Initial Status |
| User Status Changed | ApplicationUser | Admin activates/deactivates | New status, reason |
| Research Area Saved | ResearchArea | Admin creates/updates area | Area name, description |
| Research Area Toggled | ResearchArea | Admin activates/deactivates | New IsActive status |

**Non-Technical Impact:**

- ✓ **Compliance** — Audit trail proves system decisions are traceable
- ✓ **Forensics** — Admins can investigate anomalies or disputes
- ✓ **Accountability** — Every important action is attributed to a user and timestamp
- ✓ **Transparency** — Students/supervisors can see action history on their proposals

---

#### Service Dependency Graph

```
StudentController
    ↓
ProposalService ←─ IProposalRepository
    ├─ IResearchAreaRepository
    └─ IAuditService ←─ IAuditLogRepository

SupervisorController
    ↓
MatchingService ←─ IProposalRepository
    ├─ IResearchAreaRepository
    └─ IAuditService ←─ IAuditLogRepository

AdminController
    ↓
AdminService ←─ IProposalRepository
    ├─ IResearchAreaRepository
    ├─ IAuditLogRepository
    ├─ UserManager<ApplicationUser> (Identity)
    └─ IAuditService ←─ IAuditLogRepository
```

---

#### Error Handling and Validation

All services validate inputs and return structured `ServiceResult`:

```csharp
// Example from ProposalService.UpdateProposalAsync:
public async Task<ServiceResult> UpdateProposalAsync(string studentUserId, int proposalId, ProposalFormViewModel model)
{
    // Fetch proposal
    var proposal = await _proposalRepository.GetByIdAsync(proposalId);
    if (proposal == null)
        return new ServiceResult { Success = false, ErrorMessage = "Proposal not found" };
    
    // Ownership check
    if (proposal.StudentUserId != studentUserId)
        return new ServiceResult { Success = false, ErrorMessage = "Unauthorized" };
    
    // Business rule: Cannot edit matched proposals
    if (proposal.Status == ProposalStatus.Matched)
        return new ServiceResult { Success = false, ErrorMessage = "Cannot edit matched proposals" };
    
    // Validation: Research area is active
    var area = await _researchAreaRepository.GetByIdAsync(model.ResearchAreaId);
    if (area == null || !area.IsActive)
        return new ServiceResult { Success = false, ErrorMessage = "Selected research area is invalid or inactive" };
    
    // Update entity
    proposal.Title = model.Title;
    proposal.Description = model.Description;
    // ... more fields
    
    // Persist
    await _proposalRepository.UpdateAsync(proposal);
    
    // Audit
    await _auditService.RecordAsync("Proposal Updated", "Proposal", proposal.Id.ToString(), studentUserId, "...");
    
    return new ServiceResult { Success = true };
}
```

**Controllers handle results consistently:**

```csharp
var result = await _proposalService.UpdateProposalAsync(userId, id, model);
if (!result.Success)
{
    ModelState.AddModelError("", result.ErrorMessage);
    return View(model);
}

return RedirectToAction("Proposals", new { message = "Proposal updated successfully" });
```

---

#### Async/Await Usage

All repository and EF Core queries are **fully async**:

```csharp
public async Task<ServiceResult> ExpressInterestAsync(string supervisorUserId, int proposalId)
{
    // All database calls are async
    var proposal = await _proposalRepository.GetByIdAsync(proposalId);
    var existingInterest = await _proposalRepository.GetInterestAsync(proposalId, supervisorUserId);
    
    // ... validation ...
    
    var interest = new SupervisorInterest { ProposalId = proposalId, SupervisorUserId = supervisorUserId, ExpressedAt = DateTime.UtcNow };
    await _proposalRepository.AddInterestAsync(interest);
    await _auditService.RecordAsync(...);
    
    return new ServiceResult { Success = true };
}
```

**Benefits:**
- Does not block thread pool
- Scales well under load
- ASP.NET Core can serve more concurrent requests

---

#### Service Registration (Program.cs)

```csharp
builder.Services.AddScoped<IProposalRepository, ProposalRepository>();
builder.Services.AddScoped<IResearchAreaRepository, ResearchAreaRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IProposalService, ProposalService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<IAdminService, AdminService>();
```

**Scoped Lifetime:**
- Each HTTP request gets a new instance
- Instances are disposed after request completes
- Database context lives for request lifetime

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
