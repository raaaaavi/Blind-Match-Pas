# Development Notes

## Architecture Decisions

- The solution uses a standard multi-project layout with a dedicated MVC app, unit tests, and integration tests.
- ASP.NET Core Identity handles authentication, cookies, and role-based authorization.
- Controllers remain intentionally thin and delegate workflow logic to services.
- Repository abstractions are used for proposal, research area, and audit access to keep data retrieval reusable and testable.
- View models are separated from entity models to protect domain integrity and simplify Razor rendering.

## Blind-Match Logic

- Students submit proposals into a reviewable lifecycle.
- Supervisors browse proposals anonymously through the blind review browser.
- Student identity is not rendered in supervisor-facing views before a confirmed match.
- A supervisor may express interest without revealing student details.
- A confirmed match creates a `Match` record, updates proposal status to `Matched`, and enables identity reveal.
- Once matched, students cannot edit or withdraw the proposal.
- Admin reassignments create an auditable override path for exceptional cases.

## Database Design

- `ApplicationUser` extends Identity user data with display and registration metadata.
- `StudentProfile` and `SupervisorProfile` separate role-specific academic details.
- `Proposal` is the core lifecycle entity.
- `ProposalKeyword`, `SupervisorInterest`, and `SupervisorExpertise` support many-to-one and many-to-many style relations.
- `Match` stores the final confirmed allocation and reveal timestamp.
- `ProposalStatusHistory` records timeline state transitions.
- `AuditLog` records administrative and business-critical events.
- EF Core migrations are stored under `BlindMatchPAS.Web/Data/Migrations`.

## Security Considerations

- ASP.NET Core Identity enforces login, roles, and secure cookie access.
- `[Authorize]` and role restrictions protect dashboard routes.
- Student identity is withheld from blind-review pages until confirmed match.
- Sensitive POST actions use anti-forgery validation.
- Proposal ownership checks stop students from editing other students' proposals.
- Business rules block illegal transitions like editing or withdrawing matched proposals.
- Audit logging provides traceability for important actions.

## Testing Choices

- Unit tests target service rules such as:
  - inactive research area validation
  - matched proposal edit prevention
  - withdrawal prevention after match
  - duplicate interest prevention
  - double-match prevention
  - admin reassignment behavior
- Integration tests verify:
  - repository persistence
  - anonymous proposal retrieval rules
  - route protection redirect behavior

## Functional Test Journey Notes

- Student submits a proposal and sees timeline progression.
- Supervisor reviews the same proposal anonymously and can express interest.
- Supervisor confirms a match and identities become visible.
- Admin can reassign the proposal with an explicit reason and audit trail.

## Known Future Improvements

- Add richer analytics charts through a dedicated chart library.
- Add notifications and inbox-style updates per user role.
- Add pagination and more advanced server-side search/filtering.
- Add concurrency handling with explicit user feedback for stale edits.
- Add email notifications for match confirmation and reassignment events.
- Add full functional browser automation tests with Playwright or Selenium.
