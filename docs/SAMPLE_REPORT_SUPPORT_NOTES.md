# Sample Report Support Notes

## Suggested Report Positioning

- Present Blind-Match PAS as a fairness-focused academic allocation platform.
- Emphasize the blind review requirement as the central differentiator from plain CRUD systems.
- Highlight the service-layer business rules that enforce the blind-to-reveal transition.
- Use the seeded data to demonstrate multiple proposal states during screenshots and the viva.

## Functional Test Case Table

| ID | User Journey | Preconditions | Expected Result |
|---|---|---|---|
| FT-01 | Student registers | Public registration page available | Student account is created and redirected to dashboard |
| FT-02 | Student submits proposal | Student authenticated | Proposal saved with `PendingReview` status and timeline entries |
| FT-03 | Student edits proposal | Proposal not matched | Updated proposal persists successfully |
| FT-04 | Student withdraws proposal | Proposal not matched | Proposal status changes to `Withdrawn` |
| FT-05 | Supervisor browses proposals | Supervisor authenticated | Anonymous proposal data is shown without student identity |
| FT-06 | Supervisor expresses interest | Proposal available | Interest saved and proposal may move to `UnderReview` |
| FT-07 | Supervisor confirms match | Proposal unmatched | Match record created, status becomes `Matched`, identity revealed |
| FT-08 | Double match prevention | Proposal already matched | Second confirmation is blocked |
| FT-09 | Admin reassigns match | Admin authenticated | Match updated with override audit trail |
| FT-10 | Unauthorized route access | Anonymous or wrong role | Redirect to login or access denied |

## Technical Evidence to Mention

- ASP.NET Core Identity with roles for `Student`, `Supervisor`, `ModuleLeader`, and `SystemAdmin`
- Entity Framework Core code-first design with migrations
- Service-layer workflow enforcement for blind matching
- Seeded demo accounts and seeded proposal lifecycle examples
- Unit and integration tests using xUnit, Moq, SQLite, and WebApplicationFactory

## Viva Demo Sequence

1. Show the public landing page and explain the blind-matching concept.
2. Log in as a student and submit or open a proposal.
3. Log in as a supervisor and show the anonymous browser with hidden identities.
4. Confirm a match and show identity reveal.
5. Log in as admin and show oversight, reassignment, and audit logging.
