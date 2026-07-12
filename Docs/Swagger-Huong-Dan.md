# Hướng Dẫn Swagger - LMS Lab 3 Microservices

## 1. Vì sao trước đây API Gateway không có Swagger?

API Gateway ban đầu chỉ đóng vai trò reverse proxy bằng YARP. Nghĩa là Gateway nhận request từ client, kiểm tra JWT, rồi chuyển tiếp request xuống Identity Service, Student Service hoặc Course Service.

Vì Gateway không có controller nghiệp vụ riêng nên nếu không cấu hình thêm Swagger, đường dẫn `http://localhost:5000/swagger` sẽ không mở được. Các service phía sau vẫn có Swagger riêng vì chúng có controller thật.

Hiện tại Gateway đã được bổ sung Swagger riêng để mô tả các route proxy quan trọng bằng tiếng Việt. Tài liệu này không thay thế controller của service phía sau; nó giúp demo và test toàn hệ thống qua một cổng duy nhất.

## 2. Các link Swagger khi chạy Docker Compose

| Thành phần | Link |
|---|---|
| API Gateway Swagger | `http://localhost:5000/swagger` |
| Identity Service Swagger | `http://localhost:5001/swagger` |
| Student Service Swagger | `http://localhost:5002/swagger` |
| Course Service Swagger | `http://localhost:5003/swagger` |

Nên demo chính qua API Gateway Swagger để chứng minh client chỉ cần gọi một entry point.

## 3. Tài khoản seed để test

```text
username: admin
password: 123456
role: Admin
```

Mật khẩu được lưu dạng BCrypt hash trong Identity DB, không lưu plain text.

## 4. Cách test trên API Gateway Swagger

1. Mở `http://localhost:5000/swagger`.
2. Gọi `POST /api/auth/login`.
3. Body mẫu:

```json
{
  "username": "admin",
  "password": "123456"
}
```

4. Copy giá trị `data.accessToken` trong response.
5. Bấm nút **Authorize** ở đầu trang Swagger.
6. Dán access token vào ô Bearer token.
7. Gọi các API protected như:

```text
GET /api/v1/students?page=1&size=5
GET /api/v1/courses?page=1&size=10
GET /api/v1/subjects?page=1&size=5
GET /api/v1/semesters?page=1&size=5
POST /api/courses/1/enroll
```

## 5. Luồng ghi danh qua gRPC

Endpoint chính:

```text
POST /api/courses/{courseId}/enroll
```

Body mẫu:

```json
{
  "studentId": 3
}
```

Khi gọi endpoint này:

1. Client gửi request vào API Gateway.
2. Gateway kiểm tra JWT.
3. Gateway forward request xuống Course Service.
4. Course Service kiểm tra khóa học tồn tại trong Course DB.
5. Course Service gọi gRPC sang Student Service để kiểm tra sinh viên tồn tại.
6. Nếu hợp lệ, Course Service tạo bản ghi Enrollment trong Course DB.

Course Service không truy cập trực tiếp Student DB, đúng yêu cầu microservices.

## 6. Các lỗi thường gặp

| Hiện tượng | Nguyên nhân thường gặp | Cách xử lý |
|---|---|---|
| `http://localhost:5000/swagger` không mở được | Gateway chưa build lại image sau khi thêm Swagger | Chạy `docker compose up --build` |
| Gọi API protected bị `401` | Chưa bấm Authorize hoặc token sai/hết hạn | Login lại và dán access token mới |
| Gọi API admin bị `403` | Token hợp lệ nhưng role không phải Admin | Dùng tài khoản seed `admin / 123456` |
| Enrollment trả `404 Student not found` | `studentId` không tồn tại trong Student DB | Dùng student ID từ dữ liệu seed, ví dụ `2` hoặc `3` |
| Enrollment trả lỗi duplicate | Sinh viên đã ghi danh khóa học đó trước đó | Đổi `studentId` hoặc `courseId` khác |

