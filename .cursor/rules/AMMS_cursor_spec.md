# Cursor Project Specification — AMMS Admin Operational Portal
> Version 1.0 | April 2026 | ASP.NET Core Web API + React + TypeScript

---

## Project Overview

- **Backend:** ASP.NET Core Web API (.NET 8)
- **Frontend:** React + TypeScript (Vite)
- **Database:** SQL Server with SSDT project for schema management
- **ORM / Data Access:** Dapper (stored procedure-based, no EF Core)
- **Authentication:** JWT Bearer tokens with role-based authorization
- **File Storage:** Azure Blob Storage (documents, logos, event banners)
- **Messaging:** SMS + WhatsApp gateway integration (async dispatch)
- **Deployment:** Azure DevOps CI/CD pipelines
- **API Version:** `/api/v1/` prefix on all routes

---

## Coding Standards

- Use **PascalCase** for class names, method names, and properties.
- Use **camelCase** for local variables, parameters, and JSON response keys.
- Always prefer **async/await** over `Task.Result` or `.GetAwaiter().GetResult()`.
- Avoid `dynamic` and `var` unless type inference is completely obvious.
- Use **dependency injection** for all services; register in `Program.cs`.
- Never use `static` mutable state; keep services stateless.
- Use `readonly` for injected dependencies in constructors.
- All service interfaces must live in a separate `Interfaces/` folder.

---

## API Guidelines

- Follow REST conventions: **GET** (read), **POST** (create), **PUT** (update), **DELETE** (remove).
- All endpoints return a **standardised response envelope**:
  ```json
  { "success": true, "message": "...", "data": { ... } }
  { "success": false, "message": "Error description", "errors": [ ... ] }
  ```
- Use **DTOs** for all request and response models — never expose entity/table classes directly.
- Validate all inputs with **FluentValidation**; validators registered via DI.
- Use `[FromBody]` for JSON payloads, `[FromQuery]` for query parameters, `[FromRoute]` for path params.
- HTTP status codes must be semantically correct:
  - `200 OK` — successful read or update
  - `201 Created` — successful resource creation (include `Location` header)
  - `400 Bad Request` — validation failure
  - `401 Unauthorized` — missing or invalid JWT
  - `403 Forbidden` — valid JWT but insufficient role
  - `404 Not Found` — resource not found
  - `409 Conflict` — duplicate or constraint violation
  - `422 Unprocessable Entity` — business rule violation (e.g. event full)
- Paginated responses must include `total`, `page`, `pageSize`, and `records`.
- All file download endpoints (`/reports/members`, `/reports/payments`, `/audit-logs/export`) must return binary streams with correct `Content-Type` and `Content-Disposition` headers.

---

## Role Hierarchy & Authorization

Define roles as constants/enum. Roles in ascending privilege order:

| Role | Value |
|------|-------|
| Support | 1 |
| Finance | 2 |
| Manager | 3 |
| Admin | 4 |
| Super Admin | 5 |

- Annotate every controller action with `[Authorize(Policy = "MinRole:Manager")]` or equivalent custom policy.
- Implement a `RoleRequirementHandler` that reads the `role` claim from JWT and compares against the minimum required level.
- The `Support+`, `Finance+`, `Manager+`, `Admin+` patterns mean "that role or higher."
- `Admin` (exact) means only Admin and Super Admin may access.
- `Super Admin` means only Super Admin may access.

---

## Endpoint Catalogue

### 3. Admin Dashboard & Analytics

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/admin/dashboard` | Admin+ | `usp_Admin_GetDashboard` |
| GET | `/admin/analytics` | Admin+ | `usp_Admin_GetAnalytics` |
| GET | `/admin/pending-queue` | Manager+ | `usp_Admin_GetPendingQueue` |

**GET /admin/dashboard**
- Returns: `totalMembers`, `activeMembers`, `inactiveMembers`, `pendingApplications`, `onHoldApplications`, `rejectedApplications`, `currentYearRevenue`, `last7DaysRegistrations[]`
- Cache response for 2 minutes (use `IMemoryCache` or `OutputCache`).

**GET /admin/analytics**
- Query param: `year` (optional int, e.g. `2026` = FY 2025-26; default current FY)
- Returns: `membershipGrowth[]`, `monthlyRevenue[]`, `planBreakdown[]`, `yearComparison { current, previous }`

**GET /admin/pending-queue**
- Query params: `page` (default 1), `pageSize` (default 20)
- Returns FIFO-ordered list; flag applications on hold > 7 days.

---

### 4. Member Management & Verification

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/admin/members` | Manager+ | `usp_Admin_GetMembers` |
| GET | `/admin/members/:id` | Manager+ | `usp_Admin_GetMemberDetail` |
| PUT | `/admin/members/:id/verify` | Manager+ | `usp_Admin_VerifyMember` |
| PUT | `/admin/members/:id/hold` | Manager+ | `usp_Admin_HoldMember` |
| PUT | `/admin/members/:id/reject` | Manager+ | `usp_Admin_RejectMember` |
| GET | `/admin/members/:id/documents` | Manager+ | `usp_Admin_GetMemberDocuments` |

**GET /admin/members** — Query params: `page`, `pageSize`, `status`, `firmId`, `planId`, `search`, `dateFrom`, `dateTo`, `sortBy`, `sortOrder`

**PUT /admin/members/:id/verify**
- Body: `{ "notes": string }`
- Assigns Membership ID format: `AMMS-{YYYY}-{NNNN}` (zero-padded 4 digits)
- Triggers SMS + email welcome notification
- Idempotent for already-verified members

**PUT /admin/members/:id/hold**
- Body: `{ "reason": string }` — **mandatory**, minimum 20 characters
- Triggers SMS to member with hold reason

**PUT /admin/members/:id/reject**
- Body: `{ "feedback": string }` — **mandatory**
- Cannot be undone without Super Admin override
- Member can re-apply after 30 days

**GET /admin/members/:id/documents**
- Returns pre-signed blob URLs valid for 15 minutes
- Documents grouped by type: `AADHAR_FRONT`, `AADHAR_BACK`, `PHOTO`, `FIRM_REGISTRATION`, etc.
- Includes AI verification status and confidence score per document

---

### 5. Firms Management

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/admin/firms` | Manager+ | `usp_Admin_GetFirms` |
| POST | `/admin/firms` | Admin+ | `usp_Admin_CreateFirm` |
| GET | `/admin/firms/:id` | Manager+ | `usp_Admin_GetFirmDetail` |
| PUT | `/admin/firms/:id` | Admin+ | `usp_Admin_UpdateFirm` |
| DELETE | `/admin/firms/:id` | Admin (exact) | `usp_Admin_SoftDeleteFirm` |
| POST | `/admin/firms/:id/documents` | Admin+ | `usp_FirmDocument_Create` |
| POST | `/admin/firms/:id/members` | Admin+ | `usp_FirmMember_Link` |
| DELETE | `/admin/firms/:id/members/:mid` | Admin+ | `usp_FirmMember_Unlink` |

**POST /admin/firms** — Validate GST number: 15-character alphanumeric. Check uniqueness on name + GST.

**DELETE /admin/firms/:id** — Soft-delete only (`IsDeleted = true`). Return `409` if active members are linked.

**POST /admin/firms/:id/documents** — `multipart/form-data`. Supported `documentType` values: `LEASE_DEED`, `FIRM_REGISTRATION`, `GST_CERTIFICATE`, `PARTNERSHIP_DEED`, `OTHER`.

**POST /admin/firms/:id/members** — Body: `{ "memberId": string, "roleInFirm": "Owner" | "Partner" | "Employee" }`. Member must be Verified.

---

### 6. Staff & Role Management

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/admin/staff` | Admin (exact) | `usp_Admin_GetStaff` |
| POST | `/admin/staff` | Admin (exact) | `usp_Admin_CreateStaff` |
| PUT | `/admin/staff/:id` | Admin (exact) | `usp_Admin_UpdateStaff` |
| DELETE | `/admin/staff/:id` | Admin (exact) | `usp_Admin_DeactivateStaff` |
| GET | `/admin/roles` | Admin (exact) | `usp_Admin_GetRoles` |
| PUT | `/admin/roles/:role` | Super Admin | `usp_Admin_UpdateRolePermissions` |

**POST /admin/staff** *(Already Created)*
- Auto-generate 12-character complex temp password
- Set `MustChangePassword = true`
- Send onboarding email with credentials

**PUT /admin/staff/:id**
- Promoting to Super Admin requires caller to also be Super Admin
- Role change → invalidate all active JWTs for that staff (add JTI to `tbl_TokenDenylist`)

**DELETE /admin/staff/:id**
- Set `IsActive = false`; invalidate all sessions
- Cannot deactivate own account
- Admin cannot deactivate a Super Admin

**PUT /admin/roles/:role** — Super Admin only. Log before/after snapshot to `tbl_AuditLog`.

---

### 7. Broadcast & Notifications

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/admin/broadcasts` | Admin+ | `usp_Admin_GetBroadcasts` |
| POST | `/admin/broadcasts` | Admin+ | `usp_Broadcast_Create` |
| GET | `/admin/broadcasts/:id` | Admin+ | `usp_Broadcast_GetDetail` |
| DELETE | `/admin/broadcasts/:id` | Admin (exact) | `usp_Broadcast_Delete` |
| GET | `/member/notifications` | Member | `usp_Member_GetNotifications` |
| PUT | `/member/notifications/read` | Member | `usp_Member_MarkNotificationsRead` |

**POST /admin/broadcasts**
- Target filter types: `AllActiveMembers`, `SpecificPlan`, `ExpiringIn30Days`, `CustomFilter`
- Channels: `SMS`, `WhatsApp`, `Both`
- Message supports personalisation tokens: `{name}`, `{membershipId}`
- Dispatch is **asynchronous** via a background queue
- Supports optional `scheduledAt` (ISO 8601 datetime)

**GET /member/notifications** — Returns up to 50 notifications; includes `unreadCount`.

---

### 8. Audit Logs

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/admin/audit-logs` | Admin (exact) | `usp_AuditLog_Get` |
| GET | `/admin/audit-logs/export` | Admin (exact) | `usp_AuditLog_Export` |

**GET /admin/audit-logs** — Query params: `page`, `pageSize`, `staffId`, `actionType`, `entityType`, `dateFrom`, `dateTo`

**GET /admin/audit-logs/export** — Query params: `format` (`csv` | `xlsx`, default `csv`), `dateFrom`, `dateTo`, `staffId`, `actionType`. Exports > 10,000 rows must be queued and delivered via email link.

---

### 9. System Settings

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/admin/settings` | Admin (exact) | `usp_Settings_GetAll` |
| PUT | `/admin/settings/plans` | Super Admin | `usp_Settings_UpdatePlans` |
| PUT | `/admin/settings/gst` | Super Admin | `usp_Settings_UpdateGST` |
| PUT | `/admin/settings/branding` | Super Admin | `usp_Settings_UpdateBranding` |

**PUT /admin/settings/plans** — Validate: `baseFee > 0`, `platformFeePercent` between 0 and 10. Log before/after to audit.

**PUT /admin/settings/gst** — Both `cgst` and `sgst` required. Archive previous rate in `tbl_GSTRateHistory`.

**PUT /admin/settings/branding** — `multipart/form-data`. Validate GSTIN format (15-char). Logo upload to Azure Blob; store URL in settings.

---

### 10. Reports & Support Lookup

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/admin/reports/members` | Admin+ | `usp_Report_GetMembers` |
| GET | `/admin/reports/payments` | Finance+ | `usp_Report_GetPayments` |
| GET | `/admin/support/lookup` | Support+ | `usp_Support_MemberLookup` |

**GET /admin/reports/payments** — `dateFrom` and `dateTo` are **required**. Report rows include: receipt number, member name, plan, base amount, CGST, SGST, platform fee, total, payment date, Razorpay reference. Includes summary totals row.

**GET /admin/support/lookup** — Query param `q` (min 3 chars). Searches name (partial), mobile (exact), or membership ID. Returns max 10 results with masked mobile (`98XXXXX210`).

---

### 11. Engagement & Growth Tools

#### 11.1 Member Directory

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/directory/members` | Member+ | `usp_Directory_GetMembers` |
| GET | `/directory/members/:id` | Member+ | `usp_Directory_GetMemberCard` |

- Only `Verified + Active` members are shown.
- Mobile and email are **not exposed** in directory responses.

#### 11.2 Event Manager

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/events` | All (JWT) | `usp_Event_GetList` |
| POST | `/events` | Admin+ | `usp_Event_Create` |
| GET | `/events/:id` | All (JWT) | — |
| PUT | `/events/:id` | Admin+ | — |
| DELETE | `/events/:id` | Admin+ | — |
| POST | `/events/:id/register` | Member | `usp_Event_Register` |
| DELETE | `/events/:id/register` | Member | `usp_Event_CancelRegistration` |
| GET | `/events/:id/attendees` | Admin+ | `usp_Event_GetAttendees` |

- Registration uses **row-level locking** to prevent over-booking.
- Full seats → member added to waitlist.
- Cancellation triggers automatic waitlist promotion + SMS to promoted member.

#### 11.3 Referral System

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| GET | `/referrals/my-code` | Member | `usp_Referral_GetMyCode` |
| POST | `/referrals/track` | Public | `usp_Referral_Track` |
| GET | `/referrals/stats` | Member | — |
| GET | `/admin/referrals` | Admin+ | — |

- Referral code generated once and stored permanently in `tbl_Members.ReferralCode`.
- Referral status: `Pending` → `Converted` on member verification.

#### 11.4 Grievance Portal

| Method | Route | Min Role | SP |
|--------|-------|----------|----|
| POST | `/grievances` | Member | `usp_Grievance_Submit` |
| GET | `/grievances/my` | Member | `usp_Grievance_GetByMember` |
| GET | `/admin/grievances` | Admin+ | `usp_Admin_GetGrievances` |
| PUT | `/admin/grievances/:id` | Admin+ | `usp_Admin_UpdateGrievance` |

- Ticket number format: `GRV-{YYYY}-{NNNN}`
- SLA: 5 working days. Flag records past SLA in admin list view.
- Status flow: `Open → InProgress → Resolved → Closed`
- `resolutionNote` is **mandatory** for `Resolved` and `Closed` statuses (min 30 characters).
- Categories: `Membership`, `Payment`, `Service`, `Other`

---

## Database Rules

- All schema changes tracked in the **SSDT project** with `IF NOT EXISTS` guards.
- Use `nvarchar` for all text fields; never `varchar`.
- **Primary keys must be GUIDs** (`uniqueidentifier`) unless performance dictates integers.
- Every table must include `CreatedDate` (`datetime2`) and `UpdatedDate` (`datetime2`) columns.
- Soft-delete pattern: `IsDeleted bit NOT NULL DEFAULT 0` — **no physical deletes**.
- Sensitive columns (passwords, tokens): store only hashed values; never plaintext.
- All stored procedures must use parameterised inputs; no dynamic SQL string concatenation.
- Foreign key constraints required on all FK columns.
- Add appropriate indexes on filter/search columns (status, date ranges, FKs).

### Key Tables

| Table | Purpose |
|-------|---------|
| `tbl_Staff` | Staff accounts: StaffId, Name, Email, PasswordHash, Salt, RoleId, IsActive, FailedAttempts, LockedUntil, MustChangePassword, LastLogin, CreatedBy |
| `tbl_Roles` | Role definitions with `Permissions` JSON column |
| `tbl_TokenDenylist` | Invalidated JTIs for session revocation |
| `tbl_AuditLog` | LogId, StaffId, Action, EntityType, EntityId, Payload (JSON), IPAddress, CreatedAt |
| `tbl_Firms` | FirmId, Name, GSTNo, RegistrationNo, Address, ContactEmail, ContactMobile, IsDeleted, CreatedBy, CreatedAt |
| `tbl_FirmMembers` | FirmMemberId, FirmId, MemberId, RoleInFirm, IsActive, LinkedBy, LinkedAt |
| `tbl_FirmDocuments` | FirmDocId, FirmId, DocumentType, BlobUrl, UploadedBy, UploadedAt |
| `tbl_Broadcasts` | BroadcastId, Title, Message, Channel, TargetFilter (JSON), SentAt, CreatedBy, RecipientCount, DeliveredCount, FailedCount, IsDeleted |
| `tbl_BroadcastDelivery` | DeliveryId, BroadcastId, MemberId, Status, SentAt, ErrorCode |
| `tbl_Notifications` | NotificationId, MemberId, Title, Message, Category, LinkTo, IsRead, CreatedAt |
| `tbl_SystemSettings` | SettingKey, SettingValue, UpdatedBy, UpdatedAt |
| `tbl_GSTRateHistory` | HistoryId, CGST, SGST, EffectiveFrom, UpdatedBy |
| `tbl_Events` | EventId, Name, Description, EventDate, Venue, TotalSeats, RegistrationDeadline, BannerDocId, IsCancelled, CreatedBy, CreatedAt |
| `tbl_EventRegistrations` | RegistrationId, EventId, MemberId, Status (Confirmed/Waitlisted), RegisteredAt |
| `tbl_Referrals` | ReferralId, ReferrerId, RefereeMobile, RefereeMemberId (nullable), ReferralCode, Status, CreatedAt, ConvertedAt |
| `tbl_Grievances` | GrievanceId, MemberId, TicketNo, Category, Subject, Description, SupportingDocId, Status, SubmittedAt, ResolvedAt, ResolutionNote |
| `tbl_GrievanceHistory` | HistoryId, GrievanceId, OldStatus, NewStatus, Note, ChangedBy, ChangedAt |

---

## Security

- Enforce **JWT Bearer authentication** on all endpoints (except `POST /referrals/track`).
- Use **role-based authorization policies** (`[Authorize(Policy = "...")]`); never hardcode role checks in business logic.
- All active session tokens for a staff member must be invalidated on role change or deactivation — store JTI in `tbl_TokenDenylist` and validate on each request.
- **Never log** sensitive data: passwords, tokens, Aadhar numbers, bank details.
- Mask Aadhar numbers in all API responses (show last 4 digits only).
- Mask mobile numbers in support lookup responses.
- Pre-signed blob URLs for documents must expire in **15 minutes**.
- Implement account lockout after N failed login attempts (`FailedAttempts`, `LockedUntil` on `tbl_Staff`).
- Validate GSTIN format (15-character alphanumeric) on all inputs that accept it.
- `multipart/form-data` endpoints must enforce file type and size limits.

---

## Audit Logging

Every state-changing action by a staff member must write to `tbl_AuditLog`:
- `Action` — descriptive string e.g. `MemberVerified`, `StaffDeactivated`, `RolePermissionsUpdated`
- `EntityType` — `Member`, `Firm`, `Staff`, `Broadcast`, `Settings`, etc.
- `EntityId` — the affected record's primary key
- `Payload` — JSON with before/after snapshot where applicable
- `IPAddress` — extracted from request context
- Implement as a reusable `IAuditService` injected into all relevant services.

---

## Testing

- **Unit tests** required for all service-layer methods (xUnit + Moq).
- **Integration tests** for all API endpoints (use `WebApplicationFactory<Program>`).
- Mock all external dependencies: SMS gateway, WhatsApp gateway, Azure Blob Storage, email service.
- Test coverage must include: validation failure paths, role enforcement (401/403), business rule violations (409/422), and happy paths.
- Naming convention: `MethodName_Scenario_ExpectedResult` (e.g. `VerifyMember_AlreadyVerified_ReturnsOk`).

---

## Frontend Style Preferences (React + TypeScript)

- All React components must be **functional components with hooks** — no class components.
- Use **Tailwind CSS** for all styling; no inline styles, no CSS modules unless unavoidable.
- **TypeScript strict mode** enabled (`"strict": true` in `tsconfig.json`).
- Define explicit TypeScript interfaces/types for all API request and response shapes in a shared `types/` folder.
- Use **React Query (TanStack Query)** for all server-state fetching, caching, and mutation.
- Use **React Hook Form + Zod** for form validation on the frontend.
- Use **Axios** (with an interceptor for JWT injection) as the HTTP client.
- Paginated tables must use controlled pagination with `page` and `pageSize` state.
- Role-based UI visibility must read from the decoded JWT claims — never fetch role from a separate endpoint.
- File download endpoints must be handled with `blob` response type and triggered via `URL.createObjectURL`.

---

## Project Folder Structure (Backend)

```
src/
  AMMS.API/
    Controllers/
      Admin/          # Dashboard, Members, Firms, Staff, Broadcasts, Audit, Settings, Reports
      Member/         # Notifications, Directory, Events, Referrals, Grievances
    Middleware/       # JWT validation, audit logging, exception handling
    Program.cs
  AMMS.Application/
    Interfaces/       # IService contracts
    Services/         # Business logic
    DTOs/             # Request/response models
    Validators/       # FluentValidation validators
  AMMS.Infrastructure/
    Data/             # Dapper repository implementations
    Blob/             # Azure Blob Storage service
    Messaging/        # SMS / WhatsApp gateway clients
    Email/            # Email service client
  AMMS.Database/      # SSDT project
    Tables/
    StoredProcedures/
```

---

## Project Folder Structure (Frontend)

```
src/
  api/              # Axios instances, API call functions per domain
  components/       # Shared UI components (tables, modals, badges, file uploaders)
  features/         # Feature folders: dashboard, members, firms, staff, broadcasts, events, grievances
  hooks/            # Custom React Query hooks per feature
  types/            # TypeScript interfaces for API contracts
  utils/            # Formatters, date helpers, role helpers
  routes/           # React Router route definitions with role guards
  store/            # Auth context / JWT claims state
```

---

## Implementation Status Reference

| Endpoint Group | Status |
|---------------|--------|
| POST /admin/staff | ✅ Already created |
| All other endpoints listed in this spec | 🔲 Pending |

All pending endpoints must be built following the standards in this document. Cross-reference the SSDT section for the corresponding stored procedure and table for each endpoint before implementation.
