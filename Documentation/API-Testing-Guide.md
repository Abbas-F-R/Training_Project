# API Testing & Endpoint Reference Guide
### Interactive API Exploration (Scalar & Swagger UI), Payload Specifications, and Security Verification

---

## 1. Interactive Documentation & Test Automation

When running the application locally (`http://localhost:5207`), two interactive API exploration interfaces are available:

1. **Scalar Interactive Documentation (Recommended):**  
   URL: [http://localhost:5207/scalar/v1](http://localhost:5207/scalar/v1)
2. **Swagger UI:**  
   URL: [http://localhost:5207/swagger](http://localhost:5207/swagger)

### Automated Test Suite
In addition to interactive manual testing, the project includes an automated test suite covering unit, integration, validation, service, security, and controller layers:
```bash
dotnet test
```
*Current test suite status: 78 passed tests (0 failures).*

---

## 2. Authentication & Authorization Setup

All domain endpoints (except authentication) require a valid **JWT Bearer** token.

### 1. Authenticate via Login
- **Method:** `POST`
- **URL:** `http://localhost:5207/api/auth/login`
- **Headers:** `Content-Type: application/json`
- **Request Body:**
```json
{
  "userName": "admin",
  "password": "Admin@12345"
}
```

### 2. Expected Response (`200 OK`):
```json
{
  "userId": "UkLWZg9D",
  "userName": "admin",
  "fullName": "System Administrator",
  "role": "Admin",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-09-30T20:15:22.8427771Z"
}
```

### 3. Setting the Token in Testing Tools:
- **In Scalar:** Click **Auth** in the sidebar, select **Bearer**, and paste the token string.
- **In Swagger UI:** Click the green **Authorize** button at the top right, enter `Bearer YOUR_TOKEN_HERE`, and confirm.

---

## 3. Academic Departments Endpoints

### A. Lookup Dropdown Options (Unpaged)
- **Method:** `GET`
- **URL:** `http://localhost:5207/api/department/lookup`
- **Response (`200 OK`):**
```json
[
  {
    "id": "UkLWZg9D",
    "name": "Computer Science",
    "code": "CS"
  },
  {
    "id": "gbHJdmfr",
    "name": "Software Engineering",
    "code": "SE"
  }
]
```

### B. List Departments (Paginated)
- **Method:** `GET`
- **URL:** `http://localhost:5207/api/department?pageNumber=1&pageSize=10`

### C. Create Department (`Admin Only`)
- **Method:** `POST`
- **URL:** `http://localhost:5207/api/department`
- **Request Body:**
```json
{
  "name": "Cybersecurity",
  "code": "CYBER"
}
```

### D. Update Department (`Admin Only`)
- **Method:** `PUT`
- **URL:** `http://localhost:5207/api/department/{id}`
- **Request Body:**
```json
{
  "name": "Data Science & Artificial Intelligence",
  "code": "DSAI"
}
```

### E. Delete Department (`Admin Only` - Soft Delete)
- **Method:** `DELETE`
- **URL:** `http://localhost:5207/api/department/{id}`
- **Response:** `true`

---

## 4. Student Management Endpoints

### A. Enroll New Student
- **Method:** `POST`
- **URL:** `http://localhost:5207/api/student`
- **Request Body:**
```json
{
  "fullName": "Alex Mercer",
  "studentCode": "STU-2026-105",
  "email": "alex.mercer@univ.edu",
  "phoneNumber": "07712345678",
  "departmentId": "UkLWZg9D",
  "stage": 2,
  "birthDate": "2004-03-12"
}
```
- **Expected Response (`200 OK`):**
```json
{
  "id": "Xm49LK2v",
  "fullName": "Alex Mercer",
  "studentCode": "STU-2026-105",
  "email": "alex.mercer@univ.edu",
  "phoneNumber": "07712345678",
  "departmentId": "UkLWZg9D",
  "departmentName": "Computer Science",
  "departmentCode": "CS",
  "stage": 2,
  "birthDate": "2004-03-12T00:00:00",
  "createdAt": "2026-09-24T00:10:00"
}
```

### B. Get Student Profile by ID
- **Method:** `GET`
- **URL:** `http://localhost:5207/api/student/{id}`

### C. Search & Filter Students (Paginated)
- **Method:** `GET`
- **URL:** `http://localhost:5207/api/student?fullName=Alex&stage=2&pageNumber=1&pageSize=10`

### D. Update Student Profile
- **Method:** `PUT`
- **URL:** `http://localhost:5207/api/student/{id}`
- **Request Body:**
```json
{
  "fullName": "Alex Mercer Smith",
  "studentCode": "STU-2026-105",
  "email": "alex.smith@univ.edu",
  "phoneNumber": "07799998888",
  "departmentId": "UkLWZg9D",
  "stage": 3,
  "birthDate": "2004-03-12"
}
```

### E. Delete Student (`Admin Only` - Soft Delete)
- **Method:** `DELETE`
- **URL:** `http://localhost:5207/api/student/{id}`
- **Response:** `true`

---

## 5. Audit Logging (`Admin Only`)

- **Method:** `GET`
- **URL:** `http://localhost:5207/api/auditlog?pageNumber=1&pageSize=20`
- **Response:** Paginated list of operational audit logs tracking all data modifications, administrative events, and authentication attempts.

---

## 6. Security & Negative Scenario Testing

### 1. Unauthenticated Request (`401 Unauthorized`)
- Invoking `GET /api/student` without supplying a `Bearer <token>` in the `Authorization` header.
- **Result:** Immediate rejection with `401 Unauthorized`.

### 2. Unauthorized Role Privilege Escalation (`403 Forbidden`)
- Authenticate with a `User` account and attempt to invoke `DELETE /api/student/{id}` or `GET /api/auditlog`.
- **Result:** Immediate rejection with `403 Forbidden`.

### 3. Duplicate Identifier Conflict (`400 Bad Request`)
- Attempting to enroll a student with an existing `studentCode` (`STU-2026-001`).
- **Result:** `400 Bad Request` with message:
```json
{
  "message": "The student code is already in use by another active student."
}
```

### 4. Structural Validation Failure (`400 Bad Request`)
- Submitting an invalid payload (e.g., blank name, malformed email, stage out of 1–6 range).
- **Result:** RFC 9110 compliant `400 Bad Request` containing field-level validation errors.
