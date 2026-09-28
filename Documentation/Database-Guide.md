# Database Design & Architecture Guide
### Vertical Slice SQL Organization, Relational Schema, Integrity Constraints, Filtered Indexes, Views, and Stored Procedures

---

## 1. Feature-Based SQL Architecture

In alignment with the **Vertical Slice Architecture**, database scripts are co-located within their corresponding feature directories, while enterprise-wide utility procedures and seed data reside in infrastructure:

```text
├── Infrastructure/Persistence/Sql/
│   ├── 00_Base_Procedures.sql       <-- Shared helper procedures (Base_CheckDuplicate, Base_GetFirst)
│   └── 01_SeedData.sql              <-- Seed data for bootstrapping (Admin user, departments, students)
├── Features/AuditLogs/Sql/
│   ├── 01_AuditLogs_Tables_Indexes.sql  <-- AuditLogs table DDL and query performance indexes
│   └── 02_AuditLogs_Procedures.sql      <-- Stored procedures: AuditLogsInsert and AuditLogsGetAll
├── Features/Auth/Sql/
│   ├── 01_Users_Tables_Constraints_Indexes.sql  <-- Users table DDL, constraints, and filtered indexes
│   └── 02_Users_Procedures.sql                  <-- Procedures: UsersGetByUserName and UsersInsert
├── Features/Departments/Sql/
│   ├── 01_Departments_Tables_Views_Constraints_Indexes.sql  <-- Departments table, view, constraints, and indexes
│   └── 02_Departments_Procedures.sql                        <-- CRUD procedures with in-transaction audit logging
├── Features/Students/Sql/
│   ├── 01_Students_Tables_Views_Constraints_Indexes.sql     <-- Students table, view, foreign key, and indexes
│   └── 02_Students_Procedures.sql                           <-- CRUD procedures with in-transaction audit logging
└── sql/
    └── MasterMigration.sql          <-- Unified execution script ordering all migrations sequentially
```

---

## 2. Entity-Relationship Diagram (ERD)

The database schema is designed for strict referential integrity, auditability, and domain constraints:

```mermaid
erDiagram
    Departments ||--o{ Students : "FK_Students_Departments"
    Users ||--o{ Students : "CreatedBy / UpdatedBy"
    Users ||--o{ Departments : "CreatedBy / UpdatedBy"
    Users ||--o{ AuditLogs : "UserId (Action Executor)"

    AuditLogs {
        bigint Id PK
        bigint UserId FK
        nvarchar Action
        nvarchar EntityName
        nvarchar EntityId
        nvarchar Changes
        nvarchar IpAddress
        nvarchar UserAgent
        bit IsSuccess
        datetime CreatedAt
    }

    Departments {
        bigint Id PK
        nvarchar Name "CHECK LEN >= 3"
        nvarchar Code "Filtered UQ Index, CHECK"
        bit IsDeleted
        bigint CreatedBy
        datetime CreatedAt
        bigint UpdatedBy
        datetime UpdatedAt
    }

    Students {
        bigint Id PK
        nvarchar FullName
        nvarchar StudentCode "Filtered UQ Index"
        nvarchar Email "CHECK Email Format"
        nvarchar PhoneNumber
        bigint DepartmentId FK
        int Stage "CHECK 1 to 6"
        date BirthDate "CHECK <= GETDATE()"
        bit IsDeleted
        bigint CreatedBy
        datetime CreatedAt
        bigint UpdatedBy
        datetime UpdatedAt
    }

    Users {
        bigint Id PK
        nvarchar FullName
        nvarchar UserName "Filtered UQ Index"
        nvarchar PasswordHash
        nvarchar Role "CHECK Admin/User/Manager"
        bit IsActive
        bit IsDeleted
        datetime CreatedAt
    }
```

---

## 3. Schema Specifications & Integrity Constraints

### A. Audit Logging (`AuditLogs`)
- **Primary Key:** `PK_AuditLogs` (CLUSTERED on `Id`).
- **Design:** Append-only storage without update or soft-delete indicators.
- **Indexes:**
  - `IX_AuditLogs_Entity`: Composite index on `(EntityName, EntityId)` for entity-specific audit queries.
  - `IX_AuditLogs_UserId`: Index on `(UserId)` for user activity auditing.
  - `IX_AuditLogs_CreatedAt`: Descending index on `(CreatedAt DESC)` for chronological timeline rendering.
  - `IX_AuditLogs_Action`: Composite index on `(Action, IsSuccess)` for security and intrusion monitoring.

### B. User Accounts (`Users`)
- **Primary Key:** `PK_Users` (CLUSTERED on `Id`).
- **CHECK Constraints:**
  - `CK_Users_Role`: Restricts roles to authorized values (`Admin`, `User`, `Manager`).
  - `CK_Users_UserName_Length`: Enforces a minimum username length of 3 characters.

### C. Academic Departments (`Departments`)
- **Primary Key:** `PK_Departments` (CLUSTERED on `Id`).
- **CHECK Constraints:**
  - `CK_Departments_Code`: Enforces minimum length of 2 characters and prohibits internal whitespace (`Code NOT LIKE '% %'`).
  - `CK_Departments_Name`: Enforces minimum length of 3 characters.

### D. Students (`Students`)
- **Primary Key:** `PK_Students` (CLUSTERED on `Id`).
- **Foreign Key:**
  - `FK_Students_Departments`: References `Departments(Id)` with `ON DELETE NO ACTION` to prevent orphan cascades.
- **CHECK Constraints:**
  - `CK_Students_Stage`: Restricts academic stages to valid bounds (`CHECK (Stage BETWEEN 1 AND 6)`).
  - `CK_Students_Email`: Validates email address format (`CHECK (Email IS NULL OR Email LIKE '%_@__%.__%')`).
  - `CK_Students_BirthDate`: Ensures birth dates cannot be in the future (`CHECK (BirthDate IS NULL OR BirthDate <= GETDATE())`).

---

## 4. Filtered Unique Indexing Strategy

In systems utilizing **Soft Delete** (`IsDeleted = 1`), traditional unique constraints prevent re-using codes or usernames that belonged to archived records. To solve this without compromising integrity, the database leverages **Filtered Unique Indexes**:

```sql
-- Students unique active code
CREATE UNIQUE NONCLUSTERED INDEX UQ_Students_StudentCode_Active
ON Students (StudentCode)
WHERE IsDeleted = 0;

-- Departments unique active code
CREATE UNIQUE NONCLUSTERED INDEX UQ_Departments_Code_Active
ON Departments (Code)
WHERE IsDeleted = 0;

-- Users unique active username
CREATE UNIQUE NONCLUSTERED INDEX UQ_Users_UserName_Active
ON Users (UserName)
WHERE IsDeleted = 0;
```

---

## 5. Architectural View Layer

Direct table access for read operations is restricted. All queries and post-mutation responses are routed through dedicated database views (`vw_{Entity}`):

1. **Automatic Soft-Delete Filtering:** Views enforce `WHERE IsDeleted = 0` at the database engine level.
2. **Denormalized Projection:** Foreign key references are joined to project readable descriptors alongside IDs (e.g., `DepartmentName` and `DepartmentCode` in `vw_Students`).
3. **Consistent Post-Mutation Projections:** Stored procedures return the view projection immediately following an `INSERT` or `UPDATE`, ensuring the client receives fully computed data without additional roundtrips.

---

## 6. Stored Procedure Architecture

All data operations are encapsulated in compiled stored procedures:

### Department Routines
- `DepartmentsGetById`: Fetches a single active department by primary key.
- `DepartmentsGetAll`: Paged query returning metadata (`TotalCount`) and the requested page slice.
- `DepartmentsLookup`: Lightweight unpaged query (`Id`, `Name`, `Code`) designed for UI select dropdowns.
- `DepartmentsInsert`: Inserts department and writes atomic `INSERT` audit log.
- `DepartmentsUpdate`: Modifies department and writes atomic `UPDATE` audit log.
- `DepartmentsDelete`: Soft-deletes department (`IsDeleted = 1`) and writes atomic `DELETE` audit log.

### Student Routines
- `StudentsGetById`: Fetches student profile joined with department details via `vw_Students`.
- `StudentsGetAll`: Search and paged query supporting filtering by name, code, stage, and department.
- `StudentsInsert`: Inserts student record and writes atomic `INSERT` audit log.
- `StudentsUpdate`: Modifies student profile and writes atomic `UPDATE` audit log.
- `StudentsDelete`: Soft-deletes student record and writes atomic `DELETE` audit log.

### Audit Log Routines
- `AuditLogsInsert`: Procedure used by application services for security events (login success/failure).
- `AuditLogsGetAll`: Administrative paged query with filtering by action, entity, user, and date range.
