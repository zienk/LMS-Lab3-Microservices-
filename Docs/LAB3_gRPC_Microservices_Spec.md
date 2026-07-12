# LAB 3 – gRPC & Microservices Architecture — Đặc tả triển khai cho AI Agent

> File này dùng để đưa cho AI coding agent (Claude Code, Copilot Agent, Cursor...) thực thi.
> Mục tiêu: refactor LMS (Lab 2) từ monolith sang **Microservices**, giao tiếp qua **gRPC**, có **API Gateway**, giữ nguyên các yêu cầu cũ (Clean Architecture, DI, JWT, Docker, Swagger).
> **Không tự ý thêm tính năng ngoài phạm vi đề bài** (trừ phần Bonus nếu được yêu cầu). Bám sát đúng cấu trúc bên dưới.

---

## 0. Bối cảnh & Ràng buộc bắt buộc phải giữ từ Lab 2

- Clean Architecture (tách Domain / Application / Infrastructure / API layer) cho **từng service**.
- Dependency Injection xuyên suốt.
- JWT Authentication.
- Docker Deployment.
- Swagger/OpenAPI cho từng service.

---

## 1. Kiến trúc tổng thể (Microservices)

Hệ thống tách thành **tối thiểu 3 service độc lập**, mỗi service là 1 project ASP.NET Core Web API riêng, **có database riêng**. **Không được truy cập trực tiếp database của service khác** — chỉ giao tiếp qua gRPC/HTTP.

| Service | Trách nhiệm | Database |
|---|---|---|
| **Identity Service** | Authentication, Authorization, JWT Generation, Refresh Token | `identity-db` |
| **Student Service** | Student Management, Student Information | `student-db` |
| **Course Service** | Course Management, Enrollment Management | `course-db` |

Ngoài ra:
- **API Gateway** (YARP hoặc Ocelot) — điểm vào duy nhất từ client, validate JWT trước khi forward request.

### Sơ đồ tổng quan

```
Client
  |
  v
API Gateway  --(JWT validate)-->  /api/auth/*     -> Identity Service
                                   /api/students/* -> Student Service
                                   /api/courses/*  -> Course Service

Course Service --(gRPC)--> Student Service   (lấy thông tin sinh viên, verify tồn tại)
```

---

## 2. Cấu trúc thư mục solution đề xuất

```
LMS-Microservices/
├── src/
│   ├── ApiGateway/
│   │   └── LMS.ApiGateway/                # YARP hoặc Ocelot project
│   │
│   ├── Services/
│   │   ├── Identity/
│   │   │   ├── LMS.Identity.Api/
│   │   │   ├── LMS.Identity.Application/
│   │   │   ├── LMS.Identity.Domain/
│   │   │   └── LMS.Identity.Infrastructure/
│   │   │
│   │   ├── Student/
│   │   │   ├── LMS.Student.Api/
│   │   │   ├── LMS.Student.Application/
│   │   │   ├── LMS.Student.Domain/
│   │   │   ├── LMS.Student.Infrastructure/
│   │   │   └── LMS.Student.Grpc/          # gRPC Server (proto + service impl)
│   │   │
│   │   └── Course/
│   │       ├── LMS.Course.Api/
│   │       ├── LMS.Course.Application/
│   │       ├── LMS.Course.Domain/
│   │       ├── LMS.Course.Infrastructure/
│   │       └── LMS.Course.GrpcClient/     # gRPC Client gọi sang Student Service
│   │
│   └── Shared/
│       ├── LMS.Shared.Contracts/          # DTO/Proto dùng chung nếu cần
│       └── LMS.Shared.Logging/            # Serilog config dùng chung
│
├── protos/
│   └── student.proto
│
├── docker-compose.yml
├── docs/
│   └── Architecture-Report.md
└── postman/
    └── LMS-Microservices.postman_collection.json
```

---

## 3. Identity Service

### Endpoints
- `POST /api/auth/register` — đăng ký user (role: Student/Admin).
- `POST /api/auth/login` — trả về `accessToken` (JWT) + `refreshToken`.
- `POST /api/auth/refresh` — cấp lại access token từ refresh token.
- `POST /api/auth/logout` — thu hồi refresh token (tuỳ chọn).

### Yêu cầu kỹ thuật
- Hash password (BCrypt/Argon2).
- JWT: chứa claims `sub`, `email`, `role`, `exp`.
- Refresh token lưu trong `identity-db`, có cơ chế expire/revoke.
- Symmetric key hoặc RSA key ký JWT — **key/secret phải cấu hình qua biến môi trường**, không hardcode.
- Swagger tích hợp nút **Authorize** (Bearer token).

---

## 4. Student Service

### Endpoints (REST — qua Gateway)
- `GET /api/students` — (Role: Admin)
- `GET /api/students/{id}`
- `POST /api/students` — (Role: Admin)
- `PUT /api/students/{id}`
- `DELETE /api/students/{id}` — (Role: Admin)

### gRPC Server (bắt buộc)
Student Service **đóng vai trò gRPC Server**, expose service để Course Service gọi.

`protos/student.proto`:
```proto
syntax = "proto3";

option csharp_namespace = "LMS.Student.Grpc";

package student;

service StudentGrpcService {
  rpc GetStudentById (GetStudentByIdRequest) returns (StudentReply);
  rpc CheckStudentExists (CheckStudentExistsRequest) returns (CheckStudentExistsReply);
}

message GetStudentByIdRequest {
  string studentId = 1;
}

message StudentReply {
  string studentId = 1;
  string fullName = 2;
  string email = 3;
  bool isActive = 4;
}

message CheckStudentExistsRequest {
  string studentId = 1;
}

message CheckStudentExistsReply {
  bool exists = 1;
}
```

- Implement `StudentGrpcServiceImpl : StudentGrpcService.StudentGrpcServiceBase` trong `LMS.Student.Grpc`, gọi xuống Application layer (không truy cập DB trực tiếp từ gRPC layer — tuân thủ Clean Architecture).
- Cấu hình Kestrel cho endpoint gRPC (HTTP/2), ví dụ port `6001` (tách riêng với REST API port).

---

## 5. Course Service

### Endpoints (REST — qua Gateway)
- `GET /api/courses`
- `GET /api/courses/{id}`
- `POST /api/courses` — (Role: Admin)
- `PUT /api/courses/{id}` — (Role: Admin)
- `DELETE /api/courses/{id}` — (Role: Admin)
- `POST /api/courses/{id}/enroll` — **(Business flow chính, xem mục 6)**
- `GET /api/courses/{id}/enrollments`

### gRPC Client (bắt buộc)
- Course Service đóng vai trò **gRPC Client**, dùng strongly-typed client sinh từ `student.proto`.
- Tạo `IStudentGrpcClient` (interface trong Application layer) + implementation trong Infrastructure layer, đăng ký qua DI (`AddGrpcClient<StudentGrpcService.StudentGrpcServiceClient>`).
- Cấu hình địa chỉ Student Service gRPC qua `appsettings.json` / biến môi trường (ví dụ `GrpcSettings:StudentServiceUrl`), để khi chạy Docker trỏ tới `http://student-service:6001`.

---

## 6. Business Flow: Enroll Student into Course

```
Client
  |
  v
API Gateway (validate JWT)
  |
  v
Course Service: POST /api/courses/{courseId}/enroll { studentId }
  |
  v (gRPC: CheckStudentExists / GetStudentById)
  v
Student Service
  |
  v
Course Service: nếu exists = true -> tạo Enrollment record trong course-db
                nếu exists = false -> trả 404/400 "Student not found"
```

**Yêu cầu bắt buộc:**
- Verify sinh viên tồn tại **thông qua gRPC** trước khi enroll (không query thẳng student-db).
- Enrollment chỉ được tạo khi student hợp lệ.
- Trả về lỗi rõ ràng nếu: student không tồn tại, đã enroll rồi, course không tồn tại/hết chỗ (nếu có giới hạn).

---

## 7. API Gateway (YARP hoặc Ocelot)

Chọn **một** trong hai — khuyến nghị **YARP** (native .NET, dễ tích hợp JWT middleware của ASP.NET Core).

### Yêu cầu
- Route:
  - `/api/auth/*` → Identity Service
  - `/api/students/*` → Student Service
  - `/api/courses/*` → Course Service
- **Validate JWT tại Gateway** trước khi forward (dùng cùng signing key/issuer với Identity Service) — request không hợp lệ trả `401` ngay tại Gateway, không cần đi tới service đích.
- Gateway **không tự expose Swagger nghiệp vụ** nhưng có thể có health-check endpoint.
- (Tuỳ chọn nâng cao) Aggregate Swagger từ các service phía sau qua Gateway.

### Cấu hình YARP mẫu (`appsettings.json` của ApiGateway)
```json
{
  "ReverseProxy": {
    "Routes": {
      "auth-route": {
        "ClusterId": "identity-cluster",
        "Match": { "Path": "/api/auth/{**catch-all}" }
      },
      "students-route": {
        "ClusterId": "student-cluster",
        "AuthorizationPolicy": "default",
        "Match": { "Path": "/api/students/{**catch-all}" }
      },
      "courses-route": {
        "ClusterId": "course-cluster",
        "AuthorizationPolicy": "default",
        "Match": { "Path": "/api/courses/{**catch-all}" }
      }
    },
    "Clusters": {
      "identity-cluster": {
        "Destinations": { "d1": { "Address": "http://identity-service:8080/" } }
      },
      "student-cluster": {
        "Destinations": { "d1": { "Address": "http://student-service:8080/" } }
      },
      "course-cluster": {
        "Destinations": { "d1": { "Address": "http://course-service:8080/" } }
      }
    }
  }
}
```

---

## 8. Authentication & Authorization (áp dụng toàn hệ thống)

- Login trả JWT + Refresh Token (Identity Service).
- Mỗi service (Student, Course) tự validate JWT (thêm `AddAuthentication().AddJwtBearer(...)` với cùng key/issuer) — **defense in depth**, dù Gateway đã validate.
- Dùng `[Authorize]` cho endpoint cần đăng nhập, `[Authorize(Roles = "Admin")]` cho endpoint quản trị.
- Test case bắt buộc: request không có token → **401**; token hợp lệ nhưng sai role → **403**.

---

## 9. Docker Deployment

### Containers tối thiểu (đúng theo đề)
- `api-gateway`
- `identity-service`
- `student-service`
- `course-service`
- `identity-db`
- `student-db`
- `course-db`

### Dockerfile (mỗi service — mẫu chung)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 6001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LMS.<ServiceName>.Api.dll"]
```
> Lưu ý: Student Service cần expose thêm port gRPC (6001), Kestrel cấu hình 2 endpoint (HTTP REST + HTTP/2 gRPC).

### `docker-compose.yml` (khung sườn)
```yaml
version: "3.9"

services:
  identity-db:
    image: postgres:16
    environment:
      POSTGRES_DB: identitydb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    volumes:
      - identity-data:/var/lib/postgresql/data

  student-db:
    image: postgres:16
    environment:
      POSTGRES_DB: studentdb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    volumes:
      - student-data:/var/lib/postgresql/data

  course-db:
    image: postgres:16
    environment:
      POSTGRES_DB: coursedb
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    volumes:
      - course-data:/var/lib/postgresql/data

  identity-service:
    build: ./src/Services/Identity/LMS.Identity.Api
    depends_on: [identity-db]
    environment:
      ConnectionStrings__Default: "Host=identity-db;Database=identitydb;Username=postgres;Password=postgres"
    ports: ["5001:8080"]

  student-service:
    build: ./src/Services/Student/LMS.Student.Api
    depends_on: [student-db]
    environment:
      ConnectionStrings__Default: "Host=student-db;Database=studentdb;Username=postgres;Password=postgres"
    ports:
      - "5002:8080"
      - "6001:6001"

  course-service:
    build: ./src/Services/Course/LMS.Course.Api
    depends_on: [course-db, student-service]
    environment:
      ConnectionStrings__Default: "Host=course-db;Database=coursedb;Username=postgres;Password=postgres"
      GrpcSettings__StudentServiceUrl: "http://student-service:6001"
    ports: ["5003:8080"]

  api-gateway:
    build: ./src/ApiGateway/LMS.ApiGateway
    depends_on: [identity-service, student-service, course-service]
    ports: ["5000:8080"]

volumes:
  identity-data:
  student-data:
  course-data:
```

---

## 10. Logging (Serilog)

Áp dụng cho **mọi service + gateway**. Log bắt buộc:
- Request Path
- HTTP Method
- Status Code
- Execution Time (ms)

Cấu hình mẫu (`Program.cs`):
```csharp
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate:
        "{Timestamp:HH:mm:ss} [{Level}] {RequestPath} {RequestMethod} {StatusCode} {Elapsed}ms {Message}{NewLine}"));

app.UseSerilogRequestLogging(); // log Method, Path, StatusCode, Elapsed tự động
```

---

## 11. Swagger/OpenAPI

- Mỗi service (Identity, Student, Course) có Swagger riêng, bật **JWT Bearer scheme** để test protected endpoint ngay trong UI.
- Cấu hình mẫu:
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Student Service API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Nhập: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] { }
        }
    });
});
```

---

## 12. Testing — checklist bắt buộc phải demo được

| Test Case | Expected Result |
|---|---|
| Login | JWT token được sinh ra thành công |
| Protected API Access (có token hợp lệ) | Trả về 200 / dữ liệu đúng |
| Unauthorized Request (không có/token sai) | HTTP 401 |
| gRPC Communication (Course → Student) | Trả dữ liệu sinh viên thành công |
| Course Enrollment (full flow) | Enrollment được tạo thành công, có verify qua gRPC |

→ Viết **Postman Collection** cover đủ 5 case trên (kèm biến `{{accessToken}}`, `{{baseUrl}}`).

---

## 13. Deliverables (phải nộp đủ)

- [ ] Source code đầy đủ (theo cấu trúc mục 2).
- [ ] Dockerfile cho từng service.
- [ ] `docker-compose.yml` chạy được toàn bộ hệ thống bằng `docker compose up`.
- [ ] Proto files (`.proto`).
- [ ] Postman Collection (`.json`).
- [ ] **Architecture Report (2–3 trang)** gồm:
  - Service decomposition (giải thích lý do tách 3 service).
  - Database design (schema từng DB, vì sao tách riêng).
  - API Gateway configuration (routing, JWT validation).
  - gRPC communication flow (sequence diagram Course ↔ Student).

---

## 14. Bonus (+10%) — chỉ làm nếu còn thời gian, không bắt buộc

- [ ] RabbitMQ Integration (ví dụ: publish event `StudentEnrolled` từ Course Service).
- [ ] Redis Cache (cache kết quả `GetStudentById` phía Course Service).
- [ ] OpenTelemetry Distributed Tracing (trace xuyên suốt Gateway → Course → gRPC → Student).
- [ ] Polly Circuit Breaker cho gRPC Client (retry/circuit breaker khi Student Service down).

---

## 15. Thứ tự triển khai đề xuất cho AI Agent

1. Tạo solution skeleton theo cấu trúc mục 2 (chưa cần logic).
2. Implement Identity Service (Auth + JWT) trước, test độc lập qua Swagger.
3. Implement Student Service: CRUD REST + **gRPC Server**.
4. Implement Course Service: CRUD REST + **gRPC Client** gọi Student Service.
5. Implement business flow Enroll (mục 6), test bằng REST trực tiếp tới Course Service (chưa qua Gateway).
6. Implement API Gateway (YARP), cấu hình routing + JWT validation.
7. Viết Dockerfile từng service + `docker-compose.yml`, test `docker compose up --build`.
8. Thêm Serilog logging cho toàn hệ thống.
9. Hoàn thiện Swagger (JWT Authorize button) cho từng service.
10. Viết Postman Collection test đủ 5 test case (mục 12).
11. Viết Architecture Report.
12. (Nếu còn thời gian) làm Bonus.

---

### Ghi chú cho AI Agent
- Tuân thủ đúng namespace/tên project như mục 2 để tránh xung đột.
- Không dùng chung 1 database cho nhiều service.
- Mọi thông tin nhạy cảm (JWT secret, connection string) phải qua biến môi trường/`appsettings.Development.json` (gitignore), không hardcode trong code.
- Khi có mâu thuẫn giữa tài liệu này và giả định của agent, **ưu tiên bám sát đề bài gốc (Section 0–14 ở trên)**, không tự thêm microservice/tính năng ngoài phạm vi.
