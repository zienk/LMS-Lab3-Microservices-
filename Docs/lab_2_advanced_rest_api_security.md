# LAB 2 – Advanced REST API & Security

> **Môn học:** PRN232  
> **Chủ đề:** Technical Requirements & Design Standards  
> **Mục tiêu:** Tiếp tục phát triển ASP.NET Core RESTful API từ Lab 1 cho hệ thống LMS, nâng cấp theo hướng API production-ready hơn.

---

## 1. Assignment Requirement

Tiếp tục phát triển project **ASP.NET Core RESTful API** từ **Lab 1** cho hệ thống **Learning Management System (LMS)**.

Sinh viên phải **tái sử dụng và cải thiện project Lab 1**, đồng thời duy trì các yêu cầu sau:

- Kiến trúc 3 lớp
- Thiết kế RESTful API
- Docker deployment
- Swagger/OpenAPI integration
- Response format nhất quán
- Search / Sort / Paging / Field Selection / Expansion
- Coding conventions và clean architecture

Lab này tập trung xây dựng API theo hướng gần với môi trường production hơn bằng cách tích hợp:

- Content Negotiation
- Data Binding / Model Binding
- Data Validation
- Advanced Routing
- Middleware
- Authentication & Authorization
- JWT Security
- API Versioning

---

## 2. Architecture & Project Structure

Tiếp tục sử dụng kiến trúc **3-layer architecture** từ Lab 1:

```text
API Layer          -> Controllers
Service Layer      -> Business Logic
Repository Layer   -> Data Access
```

### Requirements

- Controllers không được chứa business logic.
- Repositories không được chứa business logic.
- Business rules phải được xử lý trong Service Layer.
- Dependency Injection phải được sử dụng đúng cách.
- Tiếp tục duy trì clean architecture và coding conventions hiện có.

---

## 3. Database & Domain Requirements

Tái sử dụng toàn bộ tables và APIs từ Lab 1.

Sinh viên cần bổ sung thêm các bảng liên quan đến authentication.

### Required Table

```sql
User(
    UserId int,
    Username varchar(50),
    PasswordHash varchar(255),
    Role varchar(20)
)
```

### Optional Additional Tables

- RefreshToken
- Permission
- AuditLog

---

## 4. Model Types

Tiếp tục sử dụng cách tách model từ Lab 1:

- Entity Model
- Business Model
- Request Model
- Response Model

### Additional Requirements

- Request Models phải có validation attributes.
- Entity Models không được return trực tiếp cho client.

---

## 5. Content Negotiation

API phải hỗ trợ nhiều định dạng response.

### Required Formats

- `application/json`
- `application/xml`

### Requirements

- Configure XML formatter.
- API phải return data dựa trên `Accept header`.
- Unsupported formats phải return HTTP `406 Not Acceptable`.

### Example

```http
Accept: application/json
Accept: application/xml
```

---

## 6. Data Binding / Model Binding

Sinh viên phải implement đúng ASP.NET Core model binding.

### Required Binding Types

#### Route Binding

```csharp
[HttpGet("{id:int}")]
public IActionResult GetStudent([FromRoute] int id)
```

#### Query Binding

```csharp
public IActionResult GetStudents(
    [FromQuery] StudentQueryRequest request)
```

#### Body Binding

```csharp
public IActionResult CreateStudent(
    [FromBody] CreateStudentRequest request)
```

#### Header Binding

```csharp
[FromHeader(Name = "X-Request-Id")]
```

---

## 7. Data Validation

Tất cả input từ client phải được validate.

### Required Validation Attributes

Sinh viên phải sử dụng:

- `[Required]`
- `[StringLength]`
- `[Range]`
- `[EmailAddress]`
- `[Phone]`
- `[RegularExpression]`

### Example

```csharp
public class CreateStudentRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
```

### FluentValidation

Sinh viên phải implement **FluentValidation** cho ít nhất **một Request Model**.

### Custom Validation

Sinh viên phải implement ít nhất **một custom validation rule**.

Ví dụ validate mã sinh viên theo style FPTU:

```text
SE19886
CE18793
```

---

## 8. Advanced Routing & API Versioning

Sinh viên phải implement các tính năng advanced routing.

### Required Features

#### Attribute Routing

```csharp
[Route("api/students")]
```

#### Route Constraints

```csharp
[HttpGet("{id:int}")]
```

#### Nested Resources

```http
/api/courses/{courseId}/students
```

#### Named Routes

```csharp
Name = "GetStudentById"
```

### API Versioning - Required

Sinh viên phải implement API Versioning.

### Example

```http
/api/v1/students
/api/v2/students
```

---

## 9. Middleware

Sinh viên phải implement custom middleware components.

### Required Middleware

#### 9.1. Global Exception Handling Middleware

Middleware này cần:

- Handle unhandled exceptions globally.
- Return consistent error responses.
- Không expose internal server details.

#### Example Response

```json
{
  "success": false,
  "message": "Internal server error",
  "errors": null
}
```

#### 9.2. Logging Middleware

Middleware này cần log:

- Request path
- HTTP method
- Execution time
- Response status code

---

## 10. Authentication, Authorization & JWT Security

Sinh viên phải implement JWT-based authentication và authorization.

### Authentication API

Required endpoint:

```http
POST /api/auth/login
```

### Request

```json
{
  "username": "admin",
  "password": "123456"
}
```

### Response

```json
{
  "success": true,
  "data": {
    "accessToken": "...",
    "refreshToken": "...",
    "expiresIn": 3600
  }
}
```

### JWT Requirements

Sinh viên phải:

- Generate JWT tokens.
- Validate JWT tokens.
- Configure JWT authentication middleware.
- Protect APIs bằng authorization attributes.

### Authorization

#### Protected APIs

```csharp
[Authorize]
```

#### Role-based Authorization

```csharp
[Authorize(Roles = "Admin")]
```

Ít nhất phải có **một admin-only endpoint**.

### Refresh Token

Sinh viên phải implement Refresh Token flow.

Required endpoint:

```http
POST /api/auth/refresh-token
```

### Password Security

Password không được lưu dưới dạng plain text.

Sinh viên phải dùng một trong hai cách:

- BCrypt
- ASP.NET Core PasswordHasher

---

## 11. Docker Deployment

Tiếp tục sử dụng Docker Desktop deployment từ Lab 1.

### Requirements

Project phải có:

- `Dockerfile`
- `docker-compose.yml`

Cả API và Database phải chạy thành công bằng Docker Compose.

### Additional Requirements

- JWT secret phải được cấu hình bằng environment variables.
- API phải connect đúng tới database container.

---

## 12. Swagger / OpenAPI Documentation

Tiếp tục sử dụng Swagger/OpenAPI từ Lab 1.

Swagger cần hỗ trợ thêm:

- JWT authentication testing
- Authorized API testing

### JWT Swagger Requirement

Sinh viên phải configure **Swagger Authorize button** để test JWT.

---

## 13. Checklist Cho Agent Xử Lý

Dưới đây là checklist dạng task để agent dễ xử lý project.

### Architecture

- [ ] Giữ kiến trúc 3 lớp: API / Service / Repository.
- [ ] Không đặt business logic trong Controller.
- [ ] Không đặt business logic trong Repository.
- [ ] Service Layer chịu trách nhiệm xử lý business rules.
- [ ] Cấu hình Dependency Injection đầy đủ.

### Database

- [ ] Reuse database và APIs từ Lab 1.
- [ ] Thêm bảng `User`.
- [ ] Thêm `RefreshToken` nếu implement refresh token dạng lưu DB.
- [ ] Có thể thêm `Permission` hoặc `AuditLog` nếu cần mở rộng.

### Models

- [ ] Tách rõ Entity Model, Business Model, Request Model, Response Model.
- [ ] Không return Entity Model trực tiếp cho client.
- [ ] Request Models có validation attributes.

### Content Negotiation

- [ ] Support `application/json`.
- [ ] Support `application/xml`.
- [ ] Configure XML formatter.
- [ ] Return response theo `Accept header`.
- [ ] Unsupported format return HTTP 406.

### Binding

- [ ] Có Route Binding.
- [ ] Có Query Binding.
- [ ] Có Body Binding.
- [ ] Có Header Binding với `X-Request-Id`.

### Validation

- [ ] Dùng `[Required]`.
- [ ] Dùng `[StringLength]`.
- [ ] Dùng `[Range]`.
- [ ] Dùng `[EmailAddress]`.
- [ ] Dùng `[Phone]`.
- [ ] Dùng `[RegularExpression]`.
- [ ] Implement FluentValidation cho ít nhất một Request Model.
- [ ] Implement ít nhất một custom validation rule, ví dụ mã sinh viên FPTU.

### Routing & Versioning

- [ ] Dùng Attribute Routing.
- [ ] Dùng Route Constraints.
- [ ] Có Nested Resource endpoint.
- [ ] Có Named Routes.
- [ ] Implement API Versioning.
- [ ] Có endpoint dạng `/api/v1/...`.
- [ ] Có endpoint dạng `/api/v2/...`.

### Middleware

- [ ] Implement Global Exception Handling Middleware.
- [ ] Error response format nhất quán.
- [ ] Không expose internal server details.
- [ ] Implement Request Logging Middleware.
- [ ] Log request path.
- [ ] Log HTTP method.
- [ ] Log execution time.
- [ ] Log response status code.

### Authentication & Authorization

- [ ] Implement `POST /api/auth/login`.
- [ ] Generate JWT access token.
- [ ] Generate refresh token.
- [ ] Validate JWT token.
- [ ] Configure JWT authentication middleware.
- [ ] Protect APIs bằng `[Authorize]`.
- [ ] Có ít nhất một endpoint `[Authorize(Roles = "Admin")]`.
- [ ] Implement `POST /api/auth/refresh-token`.
- [ ] Password được hash bằng BCrypt hoặc PasswordHasher.

### Docker

- [ ] Có `Dockerfile`.
- [ ] Có `docker-compose.yml`.
- [ ] API chạy được bằng Docker Compose.
- [ ] Database chạy được bằng Docker Compose.
- [ ] API connect được tới database container.
- [ ] JWT secret dùng environment variables.

### Swagger

- [ ] Swagger/OpenAPI hoạt động.
- [ ] Configure Swagger JWT authentication.
- [ ] Swagger có Authorize button.
- [ ] Test được authorized API trong Swagger.

---

## 14. Gợi Ý Thứ Tự Làm

1. Review lại Lab 1 và đảm bảo project chạy được.
2. Thêm bảng `User` và authentication-related models.
3. Tạo Request/Response models cho Auth.
4. Implement password hashing.
5. Implement login API.
6. Generate JWT access token.
7. Implement refresh token flow.
8. Cấu hình JWT authentication middleware.
9. Protect API bằng `[Authorize]` và role-based authorization.
10. Thêm Content Negotiation JSON/XML.
11. Thêm validation attributes, FluentValidation và custom validation.
12. Thêm advanced routing, nested resources và named routes.
13. Thêm API versioning.
14. Thêm global exception middleware.
15. Thêm logging middleware.
16. Cập nhật Swagger để hỗ trợ JWT.
17. Cập nhật Dockerfile và docker-compose.yml.
18. Test toàn bộ API bằng Swagger/Postman.

---

## 15. Expected Deliverables

Agent hoặc developer nên tạo/kiểm tra các output sau:

- Source code ASP.NET Core REST API đã nâng cấp từ Lab 1.
- Database có bảng `User` và bảng refresh token nếu cần.
- API có authentication, authorization và JWT security.
- API hỗ trợ JSON/XML content negotiation.
- API có validation đầy đủ.
- API có versioning.
- API có custom middleware.
- Swagger test được JWT.
- Docker Compose chạy được API và Database.
- Response format nhất quán.

---

## 16. Tiêu Chí Chấm Điểm (Grading Rubric)

> **Tổng điểm tối đa:** 12.50 điểm

### Bảng Tiêu Chí

| # | Tiêu Chí | Điểm | Rule / Endpoint | Ghi Chú |
|---|----------|------|-----------------|---------|
| 1 | Solution có đúng 3 file `.csproj` (kiến trúc 3 lớp) | 0.50 | `project-count:3` | |
| 2 | Có file `docker-compose.yml` | 0.50 | `file-exists:**/docker-compose.yml` | |
| 3 | FluentValidation với `AbstractValidator` | 0.50 | `file-contains:**/Validators/*.cs:AbstractValidator` | |
| 4 | Global Exception Handling Middleware tồn tại | 0.50 | `file-exists:**/Middleware/*Exception*.cs` | Tên file phải chứa `Exception` trong thư mục `Middleware` |
| 5 | `POST /api/auth/login` trả về JWT access token & refresh token | 0.50 | `POST /api/auth/login` | Endpoint **không có** version prefix `/v1` |
| 6 | `POST /api/auth/refresh-token` trả về access token mới | 0.50 | `POST /api/auth/refresh-token` | Endpoint **không có** version prefix `/v1` |
| 7 | `POST /api/v1/auth/login` trả về JWT access token & refresh token | 1.50 | `POST /api/v1/auth/login` | Endpoint **có** version prefix `/v1` |
| 8 | `POST /api/v1/auth/refresh-token` trả về access token mới | 1.00 | `POST /api/v1/auth/refresh-token` | Endpoint **có** version prefix `/v1` |
| 9 | `GET /api/v1/students` với pagination (API v1) | 1.00 | `GET /api/v1/students?page=1&size=5` | |
| 10 | `GET /api/v2/students` với pagination (API v2) | 1.00 | `GET /api/v2/students?page=1&size=5` | |
| 11 | `GET /api/v1/courses` với pagination | 1.00 | `GET /api/v1/courses?page=1&size=10` | |
| 12 | `GET /api/v1/subjects` với pagination | 1.00 | `GET /api/v1/subjects?page=1&size=5` | |
| 13 | `GET /api/v1/semesters` với pagination | 1.00 | `GET /api/v1/semesters?page=1&size=5` | |
| 14 | `POST /api/v1/enrollments` validate Body Binding & Request Model | 0.50 | `POST /api/v1/enrollments` → HTTP 201 | |

### Ghi Chú Quan Trọng

> [!IMPORTANT]
> **Tiêu chí 4** – File Middleware phải được đặt đúng thư mục `Middleware/` và tên file phải chứa từ `Exception` (ví dụ: `ExceptionMiddleware.cs`, `GlobalExceptionHandlingMiddleware.cs`).

> [!IMPORTANT]
> **Tiêu chí 5 & 6** – Grader kiểm tra endpoint **không có** version prefix (`/api/auth/login`). Nếu project chỉ expose endpoint dạng `/api/v1/auth/login` thì sẽ không được điểm ở tiêu chí 5 & 6.

> [!TIP]
> Để đạt điểm tối đa, hãy expose **cả hai** dạng endpoint:
> - `/api/auth/login` (không có version)
> - `/api/v1/auth/login` (có version)

---

## 17. Kết Quả Mẫu – SE193418

> **Tổng điểm đạt được:** 9.50 / 12.50  
> **Trạng thái:** Done ✅

| # | Tiêu Chí | Điểm Đạt | Điểm Tối Đa | Trạng Thái | Ghi Chú Lỗi |
|---|----------|----------|-------------|------------|-------------|
| 1 | 3 file `.csproj` (3-layer) | 0.50 | 0.50 | ✅ | |
| 2 | `docker-compose.yml` tồn tại | 0.50 | 0.50 | ✅ | |
| 3 | FluentValidation `AbstractValidator` | 0.50 | 0.50 | ✅ | |
| 4 | Global Exception Middleware | 0.00 | 0.50 | ❌ | Không tìm thấy file `**/Middleware/*Exception*.cs` |
| 5 | `POST /api/auth/login` → JWT | 0.00 | 0.50 | ❌ | Status 401 – Expected 200 |
| 6 | `POST /api/auth/refresh-token` | 0.00 | 0.50 | ❌ | Status 401 – Expected 200 |
| 7 | `POST /api/v1/auth/login` → JWT | 1.50 | 1.50 | ✅ | |
| 8 | `POST /api/v1/auth/refresh-token` | 1.00 | 1.00 | ✅ | |
| 9 | `GET /api/v1/students` (pagination) | 1.00 | 1.00 | ✅ | |
| 10 | `GET /api/v2/students` (pagination) | 1.00 | 1.00 | ✅ | |
| 11 | `GET /api/v1/courses` (pagination) | 1.00 | 1.00 | ✅ | |
| 12 | `GET /api/v1/subjects` (pagination) | 1.00 | 1.00 | ✅ | |
| 13 | `GET /api/v1/semesters` (pagination) | 1.00 | 1.00 | ✅ | |
| 14 | `POST /api/v1/enrollments` (validate) | 0.50 | 0.50 | ✅ | |
| | **TỔNG** | **9.50** | **12.50** | | |

### Phân Tích Lỗi

#### ❌ Lỗi 1 – Global Exception Middleware (−0.50đ)
- **Nguyên nhân:** Grader tìm file theo pattern `**/Middleware/*Exception*.cs` nhưng không thấy.
- **Cách sửa:** Đổi tên file middleware thành `ExceptionMiddleware.cs` hoặc `GlobalExceptionHandlingMiddleware.cs` và đặt trong thư mục `Middleware/`.

#### ❌ Lỗi 2 & 3 – Endpoint không có version prefix (−1.00đ)
- **Nguyên nhân:** Grader gọi `POST /api/auth/login` (không có `/v1`) và nhận HTTP 401.
- **Nguyên nhân sâu hơn:** Project chỉ expose endpoint dạng `/api/v1/auth/login`, không có route alias không-version.
- **Cách sửa:** Thêm route không có version prefix hoặc map thêm alias:
  ```csharp
  // Option 1: Thêm route attribute thứ hai
  [HttpPost("/api/auth/login")]
  [HttpPost("/api/v1/auth/login")]
  public IActionResult Login([FromBody] LoginRequest request) { ... }
  
  // Option 2: Dùng API Versioning với default version
  options.AssumeDefaultVersionWhenUnspecified = true;
  options.DefaultApiVersion = new ApiVersion(1, 0);
  ```

---

## 18. Seed Data Tài Khoản – Bắt Buộc Để Thầy Test

> [!CAUTION]
> Đây là điều kiện **tiên quyết** để đạt điểm các tiêu chí liên quan đến authentication. Nếu seed sai tài khoản hoặc sai cách hash password, grader sẽ nhận HTTP 401 và bạn mất điểm toàn bộ phần auth.

### Tài Khoản Grader Sử Dụng Để Test

| Trường | Giá Trị |
|--------|---------|
| **Username** | `admin` |
| **Password** (plain text) | `123456` |
| **Role** | `Admin` |

> [!WARNING]
> Password **KHÔNG** được lưu plain text `123456` trực tiếp vào database. Phải lưu dưới dạng **hash**. Grader sẽ gửi `123456` và API của bạn phải tự hash rồi so sánh.

### Cách Seed Đúng

#### Sử dụng BCrypt

```csharp
// Cài package: BCrypt.Net-Next
// Install-Package BCrypt.Net-Next

var passwordHash = BCrypt.Net.BCrypt.HashPassword("123456");
// Lưu passwordHash vào database, KHÔNG lưu "123456"
```

#### Sử dụng ASP.NET Core PasswordHasher

```csharp
var hasher = new PasswordHasher<User>();
var passwordHash = hasher.HashPassword(new User(), "123456");
```

### Seed Data Trong Code – Các Cách Phổ Biến

#### Cách 1: Seed trong `DbContext.OnModelCreating` (EF Core)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Seed tài khoản admin
    modelBuilder.Entity<User>().HasData(new User
    {
        UserId = 1,
        Username = "admin",
        // Hash của "123456" dùng BCrypt (generate trước, paste vào đây)
        PasswordHash = "$2a$11$eCqb5v1j2lRVJNk7sT.4.OOVqHzJ5D7sXeKpCfJXWmhRkN3yUW5WC",
        Role = "Admin"
    });
}
```

> [!TIP]
> Để tạo hash BCrypt cho `123456`, chạy đoạn code sau một lần và copy kết quả vào seed data:
> ```csharp
> Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("123456"));
> // Output ví dụ: $2a$11$...
> ```

#### Cách 2: Seed trong `Program.cs` khi khởi động

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // hoặc EnsureCreated()

    if (!db.Users.Any(u => u.Username == "admin"))
    {
        db.Users.Add(new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
            Role = "Admin"
        });
        db.SaveChanges();
    }
}
```

#### Cách 3: Seed bằng SQL Script (nếu dùng raw SQL)

```sql
-- Chạy script này khi init database
-- Hash BCrypt của "123456": $2a$11$eCqb5v1j2lRVJNk7sT.4.OOVqHzJ5D7sXeKpCfJXWmhRkN3yUW5WC
INSERT INTO [User] (Username, PasswordHash, Role)
VALUES ('admin', '$2a$11$eCqb5v1j2lRVJNk7sT.4.OOVqHzJ5D7sXeKpCfJXWmhRkN3yUW5WC', 'Admin');
```

> [!WARNING]
> Hash BCrypt thay đổi mỗi lần generate (do salt ngẫu nhiên). Hãy tự generate hash của bạn và dùng nhất quán. **Không copy hash từ tài liệu này** vì nó chỉ là ví dụ minh họa.

### Kiểm Tra Login Thủ Công Trước Khi Nộp

Trước khi nộp bài, hãy test thủ công bằng Postman hoặc Swagger:

```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "123456"
}
```

**Response mong đợi (HTTP 200):**

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "...",
    "expiresIn": 3600
  }
}
```

> [!IMPORTANT]
> Nếu nhận được HTTP **401** khi test với `admin/123456` thì bài sẽ mất **3.00 điểm** (tiêu chí 5, 6, 7). Phải đảm bảo login hoạt động trước khi nộp.

### Checklist Seed Data

- [ ] Bảng `User` đã được tạo trong database.
- [ ] Tài khoản `admin` đã được seed với password hash của `123456`.
- [ ] Đã test `POST /api/v1/auth/login` với `admin/123456` → nhận HTTP 200 + JWT token.
- [ ] Đã test `POST /api/auth/login` (không version) với `admin/123456` → nhận HTTP 200 + JWT token.
- [ ] Seed chạy tự động khi container Docker khởi động (không cần chạy script tay).
