# LMS Lab 3 - Microservices

LMS refactored from Lab 1 and Lab 2 into microservices with Identity, Student, Course, gRPC, YARP API Gateway, JWT, Swagger, SQL Server, and Docker Compose.

## Run

```powershell
docker compose up --build
```

## Ports

| Component | URL |
|---|---|
| API Gateway | `http://localhost:5000` |
| Identity Service Swagger | `http://localhost:5001/swagger` |
| Student Service Swagger | `http://localhost:5002/swagger` |
| Course Service Swagger | `http://localhost:5003/swagger` |
| Student gRPC | `http://localhost:6001` |

## Seed Account

```text
username: admin
password: 123456
role: Admin
```

## Main Flow

1. Login through `POST /api/auth/login`.
2. Use the returned JWT as `Bearer {token}`.
3. Call protected APIs through the gateway, for example `GET /api/v1/students?page=1&size=5`.
4. Enroll a student with `POST /api/courses/1/enroll`; Course Service validates the student through Student Service gRPC before inserting the enrollment.

Postman collection: `postman/LMS-Microservices.postman_collection.json`.
Architecture report: `Docs/Architecture-Report.md`.
