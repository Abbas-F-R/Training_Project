# Audit Logging & System Activity Tracking Guide
### Operational Accountability, Append-Only Storage, In-Transaction Auditing, and Data Integrity

---

## 1. Purpose & Core Principles

In an enterprise-grade backend system, completing domain operations is insufficient without comprehensive operational traceability. The system must reliably answer critical auditing and compliance questions:
- **Who** executed the action? (`UserId`)
- **What** operation was performed? (`Action`: INSERT, UPDATE, DELETE, LOGIN_SUCCESS, LOGIN_FAILED)
- **Which** entity and record was affected? (`EntityName` and `EntityId`)
- **When** did the event occur? (`CreatedAt`)
- **What data** was provided or modified? (`Changes` JSON payload)
- **Was** the operation successful? (`IsSuccess`)

### Taxonomy: Audit Logs vs. Application Logs vs. Error Logs

| Dimension | Audit Logs | Application Logs | Error / Diagnostic Logs |
|---|---|---|---|
| **Primary Audience** | System Administrators, Compliance Officers, Security Auditors | Software Engineers, DevOps Team | Support Engineers, On-call Developers |
| **Storage Destination** | Persistent Relational Database Table (`AuditLogs`) | Text logs, Seq, Elasticsearch, CloudWatch | Sentry, Application Insights, Log files |
| **Data Nature** | Business events, state mutations, and security events | Request execution metrics, traces, performance stats | Unhandled exceptions, stack traces, crash dumps |
| **Immutability & Retention** | **Append-Only, strictly immutable (No updates or deletes)** | Rolling window retention (e.g., 30–90 days) | Retained until bug resolution |

---

## 2. Table Design & Immutability

The `AuditLogs` table is designed with immutability, high write throughput, and indexing for common compliance queries:

```sql
CREATE TABLE AuditLogs
(
    Id          BIGINT IDENTITY(1,1) NOT NULL,
    UserId      BIGINT               NULL,         -- Nullable to accommodate unauthenticated events (e.g., failed logins)
    Action      NVARCHAR(50)         NOT NULL,     -- INSERT, UPDATE, DELETE, LOGIN_SUCCESS, LOGIN_FAILED
    EntityName  NVARCHAR(100)        NOT NULL,     -- Departments, Students, Auth, Users
    EntityId    NVARCHAR(100)        NULL,         -- Identifier of the affected record
    Changes     NVARCHAR(MAX)        NULL,         -- JSON payload containing modified fields
    IpAddress   NVARCHAR(50)         NULL,         -- Client IP address
    UserAgent   NVARCHAR(255)        NULL,         -- Client browser or integration agent
    IsSuccess   BIT                  DEFAULT 1 NOT NULL, -- Operational outcome
    CreatedAt   DATETIME             DEFAULT GETDATE() NOT NULL,

    CONSTRAINT PK_AuditLogs PRIMARY KEY CLUSTERED (Id)
);
```

### Architectural Decisions:
1. **Nullable `UserId`:** Required to trace critical security events that occur prior to identity establishment (such as credential stuffing or brute-force attempts on `/api/auth/login`).
2. **`Action` and `EntityName` Categorization:** Enables targeted querying (e.g., auditing all `DELETE` actions executed against `Students` within a date range).
3. **Redacted `Changes` Payloads:** The JSON record includes relevant modified properties while strictly excluding sensitive data such as password hashes, refresh tokens, and encryption secrets.
4. **Append-Only Immutability:** The table omits `UpdatedAt` and `IsDeleted` columns. No stored procedures or application endpoints exist to edit or remove records from `AuditLogs`.

---

## 3. Atomic In-Transaction Auditing

### The Atomicity Guarantee
Decoupling audit writing from domain data mutations creates a critical consistency risk: if the host application crashes after modifying a student record but before persisting the audit entry, data has mutated without any record of who performed the modification.

To ensure strict ACID compliance, the system adopts **In-Transaction Database Auditing**:
- Both the domain modification and the audit record insertion occur within the **same atomic database transaction**.
- If the domain operation succeeds, the audit log commits simultaneously.
- If a constraint violation or database failure occurs, both the mutation and the audit insertion are rolled back atomically via `ROLLBACK`.

### Implementation Pattern (`StudentsInsert`):
```sql
CREATE OR ALTER PROCEDURE StudentsInsert
    @FullName     NVARCHAR(150),
    @StudentCode  NVARCHAR(50),
    @Email        NVARCHAR(100) = NULL,
    @PhoneNumber  NVARCHAR(30)  = NULL,
    @DepartmentId BIGINT,
    @Stage        INT = 1,
    @BirthDate    DATE = NULL,
    @CreatedBy    BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NewId BIGINT;

    -- 1. Execute domain insertion
    INSERT INTO Students (FullName, StudentCode, Email, PhoneNumber, DepartmentId, Stage, BirthDate, IsDeleted, CreatedBy, CreatedAt)
    VALUES (@FullName, @StudentCode, @Email, @PhoneNumber, @DepartmentId, @Stage, @BirthDate, 0, @CreatedBy, GETDATE());

    SET @NewId = SCOPE_IDENTITY();

    -- 2. Persist audit trail atomically within the same transaction scope
    INSERT INTO AuditLogs (UserId, Action, EntityName, EntityId, Changes, IsSuccess, CreatedAt)
    VALUES (
        @CreatedBy,
        'INSERT',
        'Students',
        CAST(@NewId AS NVARCHAR(100)),
        CONCAT(
            N'{"FullName":"', REPLACE(@FullName, '"', '\"'),
            N'","StudentCode":"', REPLACE(@StudentCode, '"', '\"'),
            N'","DepartmentId":', @DepartmentId,
            N',"Stage":', @Stage,
            N'}'
        ),
        1,
        GETDATE()
    );

    SELECT * FROM vw_Students WHERE Id = @NewId;
END
GO
```

---

## 4. Stored Procedure Auditing Matrix

The table below outlines auditing behavior across all database routines:

| Stored Procedure | Operation | Audited? | Architectural Rationale |
|---|---|---|---|
| `DepartmentsInsert` | `INSERT` | **Yes (In-Procedure)** | Department creation modifies academic taxonomy. |
| `DepartmentsUpdate` | `UPDATE` | **Yes (In-Procedure)** | Code or name modifications alter administrative reference data. |
| `DepartmentsDelete` | `DELETE` | **Yes (In-Procedure)** | Soft-deletion is a high-impact operation. |
| `DepartmentsGetById` | `SELECT` | **No** | Read operations do not mutate system state. |
| `DepartmentsGetAll` | `SELECT` | **No** | Paged queries generate high volume with zero state mutation. |
| `DepartmentsLookup` | `SELECT` | **No** | Lookup queries for UI dropdowns are read-only. |
| `StudentsInsert` | `INSERT` | **Yes (In-Procedure)** | Student admission and enrollment records are regulatory. |
| `StudentsUpdate` | `UPDATE` | **Yes (In-Procedure)** | Academic progress and profile alterations must be tracked. |
| `StudentsDelete` | `DELETE` | **Yes (In-Procedure)** | Student archival/deletion requires strict accountability. |
| `StudentsGetById` | `SELECT` | **No** | Read operation. |
| `StudentsGetAll` | `SELECT` | **No** | Read operation. |
| `UsersInsert` | `INSERT` | **Yes (In-Procedure)** | User account creation (captures username and role, excludes hash). |
| `UsersGetByUserName` | `SELECT` | **No** | Identity lookup during authentication. |
| `AuditLogsInsert` | `INSERT` | **No** | Direct logging procedure (prevents recursive logging loops). |
| `AuditLogsGetAll` | `SELECT` | **No** | Administrative inspection of audit logs. |
| **Authentication Events** | `LOGIN` | **Yes (Via Service)** | `LOGIN_SUCCESS` and `LOGIN_FAILED` tracked for intrusion detection. |

---

## 5. Audit Log API Endpoint (`Admin Only`)

- **Route:** `GET /api/auditlog`
- **Authorization:** `[Authorize(Roles = "Admin")]` (Unauthorized or non-admin callers receive `401` or `403`).
- **Query Parameters:** `pageNumber` (int), `pageSize` (int), `action` (string), `entityName` (string), `userId` (long/sqid).

### Sample Response Payload (Localized to Arabic / English)
```json
{
  "data": [
    {
      "id": "UkLWZg9D",
      "userId": "UkLWZg9D",
      "action": "INSERT",
      "localizedAction": "إضافة",
      "entityName": "Students",
      "localizedEntityName": "الطلاب",
      "entityId": "1",
      "description": "إضافة سجل جديد في الطلاب (معرف: 1)",
      "changes": "{\"FullName\":\"John Doe\",\"StudentCode\":\"STU-2026-001\",\"DepartmentId\":1,\"Stage\":3}",
      "ipAddress": null,
      "userAgent": null,
      "isSuccess": true,
      "createdAt": "2026-09-24T00:15:30"
    },
    {
      "id": "aBcD1234",
      "userId": null,
      "action": "LOGIN_FAILED",
      "localizedAction": "فشل تسجيل الدخول",
      "entityName": "Auth",
      "localizedEntityName": "المصادقة",
      "entityId": "unauthorized_user",
      "description": "محاولة تسجيل دخول فاشلة للمستخدم 'unauthorized_user'",
      "changes": "{\"Reason\":\"InvalidCredentials\"}",
      "ipAddress": null,
      "userAgent": null,
      "isSuccess": false,
      "createdAt": "2026-09-24T00:14:12"
    }
  ],
  "pagesCount": 1,
  "currentPage": 1,
  "totalCount": 2,
  "isLast": true
}
```

---

## 6. Audit Log Localization Architecture

To provide an optimal auditing experience across multilingual environments without breaking programmatic querying or filtering, the Audit Log feature implements non-destructive bilingual enrichment:

1. **Dual Preservation:**
   - Raw database identifiers (`Action`: `INSERT`, `UPDATE`, `DELETE`, `LOGIN_SUCCESS`, `LOGIN_FAILED` and `EntityName`: `Students`, `Departments`, `Auth`, `Users`) are preserved intact, enabling exact query filtering.
   - Complementary localized fields (`LocalizedAction`, `LocalizedEntityName`, and `Description`) are populated based on the requested language (`ar` or `en`).

2. **Language Preference Resolution:**
   - Language is dynamically extracted in `CurrentUser.Lang` with the following precedence:
     1. `Accept-Language` HTTP request header (e.g., `Accept-Language: ar-EG` or `ar`).
     2. Authenticated JWT token `Lang` claim.
     3. Fallback to default (`en`).

3. **Human-Friendly Event Descriptions:**
   - `AuditLogLocalizer` constructs contextual, grammatically natural summary descriptions (e.g., `"إضافة سجل جديد في الطلاب (معرف: 1)"` or `"Created new record in Students (ID: 1)"`) for rapid visual scanning in administrative portals.

