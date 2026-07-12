using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LMS.ApiGateway.Swagger;

public class GatewaySwaggerDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        AddHealth(swaggerDoc);
        AddAuth(swaggerDoc);
        AddStudents(swaggerDoc);
        AddCourses(swaggerDoc);
        AddSubjectsAndSemesters(swaggerDoc);
        AddEnrollments(swaggerDoc);
    }

    private static void AddHealth(OpenApiDocument doc)
    {
        AddOperation(doc, "/health", OperationType.Get, new OpenApiOperation
        {
            Tags = [Tag("Gateway")],
            Summary = "Kiểm tra trạng thái API Gateway",
            Description = "Endpoint nhẹ để kiểm tra Gateway đang chạy. Endpoint này không cần JWT.",
            Responses = Responses("200", "Gateway đang hoạt động.")
        });
    }

    private static void AddAuth(OpenApiDocument doc)
    {
        AddOperation(doc, "/api/auth/register", OperationType.Post, new OpenApiOperation
        {
            Tags = [Tag("Identity")],
            Summary = "Đăng ký tài khoản",
            Description = "Tạo tài khoản mới với role `Student` hoặc `Admin`. Mật khẩu được Identity Service hash bằng BCrypt.",
            RequestBody = JsonBody(ObjectSchema(("username", "string"), ("email", "string"), ("password", "string"), ("role", "string"))),
            Responses = Responses("200", "Đăng ký thành công và trả về access token + refresh token.", "400", "Dữ liệu không hợp lệ.")
        });

        AddOperation(doc, "/api/auth/login", OperationType.Post, new OpenApiOperation
        {
            Tags = [Tag("Identity")],
            Summary = "Đăng nhập",
            Description = "Đăng nhập bằng tài khoản đã seed sẵn `admin / 123456` hoặc tài khoản đã đăng ký. Dùng `accessToken` nhận được để bấm nút Authorize.",
            RequestBody = JsonBody(ObjectSchema(("username", "string"), ("password", "string"))),
            Responses = Responses("200", "Đăng nhập thành công, trả về JWT access token và refresh token.", "401", "Sai username hoặc password.")
        });

        AddOperation(doc, "/api/v1/auth/login", OperationType.Post, new OpenApiOperation
        {
            Tags = [Tag("Identity")],
            Summary = "Đăng nhập API v1",
            Description = "Route versioned tương đương `/api/auth/login`, dùng để đáp ứng checklist Lab 2.",
            RequestBody = JsonBody(ObjectSchema(("username", "string"), ("password", "string"))),
            Responses = Responses("200", "Đăng nhập thành công.", "401", "Sai username hoặc password.")
        });

        AddOperation(doc, "/api/auth/refresh-token", OperationType.Post, new OpenApiOperation
        {
            Tags = [Tag("Identity")],
            Summary = "Cấp lại access token",
            Description = "Gửi refresh token còn hiệu lực để nhận access token mới. Refresh token cũ sẽ bị thu hồi.",
            RequestBody = JsonBody(ObjectSchema(("refreshToken", "string"))),
            Responses = Responses("200", "Cấp token mới thành công.", "401", "Refresh token không hợp lệ hoặc đã hết hạn.")
        });

        AddOperation(doc, "/api/auth/logout", OperationType.Post, new OpenApiOperation
        {
            Tags = [Tag("Identity")],
            Summary = "Đăng xuất",
            Description = "Thu hồi refresh token hiện tại.",
            RequestBody = JsonBody(ObjectSchema(("refreshToken", "string"))),
            Responses = Responses("200", "Đăng xuất thành công.")
        });
    }

    private static void AddStudents(OpenApiDocument doc)
    {
        AddOperation(doc, "/api/v1/students", OperationType.Get, Protected(new OpenApiOperation
        {
            Tags = [Tag("Students")],
            Summary = "Danh sách sinh viên API v1",
            Description = "Lấy danh sách sinh viên có phân trang, tìm kiếm và sắp xếp. Cần JWT hợp lệ.",
            Parameters = [Query("page", "Số trang, bắt đầu từ 1.", "integer"), Query("size", "Số bản ghi mỗi trang.", "integer"), Query("search", "Từ khóa theo tên, email hoặc mã sinh viên."), Query("sort", "Sắp xếp, ví dụ `fullName` hoặc `-fullName`.")],
            Responses = Responses("200", "Lấy danh sách thành công.", "401", "Thiếu hoặc sai JWT.")
        }));

        AddOperation(doc, "/api/v2/students", OperationType.Get, Protected(new OpenApiOperation
        {
            Tags = [Tag("Students")],
            Summary = "Danh sách sinh viên API v2",
            Description = "Route version 2 dùng để chứng minh API versioning. Hiện trả dữ liệu tương tự v1.",
            Parameters = [Query("page", "Số trang.", "integer"), Query("size", "Số bản ghi mỗi trang.", "integer"), Query("search", "Từ khóa tìm kiếm."), Query("sort", "Sắp xếp.")],
            Responses = Responses("200", "Lấy danh sách thành công.", "401", "Thiếu hoặc sai JWT.")
        }));

        AddOperation(doc, "/api/students/{id}", OperationType.Get, Protected(new OpenApiOperation
        {
            Tags = [Tag("Students")],
            Summary = "Chi tiết sinh viên",
            Description = "Lấy thông tin một sinh viên theo ID.",
            Parameters = [Path("id", "ID sinh viên.")],
            Responses = Responses("200", "Tìm thấy sinh viên.", "404", "Không tìm thấy sinh viên.")
        }));

        AddOperation(doc, "/api/students", OperationType.Post, Protected(new OpenApiOperation
        {
            Tags = [Tag("Students")],
            Summary = "Tạo sinh viên mới",
            Description = "Chỉ role `Admin` được tạo sinh viên. Mã sinh viên phải đúng định dạng FPTU như `SE190001`.",
            RequestBody = JsonBody(ObjectSchema(("fullName", "string"), ("email", "string"), ("studentCode", "string"), ("dateOfBirth", "string"))),
            Responses = Responses("201", "Tạo sinh viên thành công.", "403", "JWT hợp lệ nhưng không có role Admin.")
        }));

        AddOperation(doc, "/api/students/{id}", OperationType.Put, Protected(new OpenApiOperation
        {
            Tags = [Tag("Students")],
            Summary = "Cập nhật sinh viên",
            Description = "Cập nhật thông tin sinh viên theo ID.",
            Parameters = [Path("id", "ID sinh viên.")],
            RequestBody = JsonBody(ObjectSchema(("fullName", "string"), ("email", "string"), ("studentCode", "string"), ("dateOfBirth", "string"))),
            Responses = Responses("200", "Cập nhật thành công.", "404", "Không tìm thấy sinh viên.")
        }));

        AddOperation(doc, "/api/students/{id}", OperationType.Delete, Protected(new OpenApiOperation
        {
            Tags = [Tag("Students")],
            Summary = "Xóa sinh viên",
            Description = "Chỉ role `Admin` được xóa sinh viên.",
            Parameters = [Path("id", "ID sinh viên.")],
            Responses = Responses("200", "Xóa thành công.", "403", "JWT hợp lệ nhưng không có role Admin.")
        }));
    }

    private static void AddCourses(OpenApiDocument doc)
    {
        AddOperation(doc, "/api/v1/courses", OperationType.Get, Protected(new OpenApiOperation
        {
            Tags = [Tag("Courses")],
            Summary = "Danh sách khóa học",
            Description = "Lấy danh sách khóa học có phân trang.",
            Parameters = [Query("page", "Số trang.", "integer"), Query("size", "Số bản ghi mỗi trang.", "integer")],
            Responses = Responses("200", "Lấy danh sách thành công.")
        }));

        AddOperation(doc, "/api/courses/{id}", OperationType.Get, Protected(new OpenApiOperation
        {
            Tags = [Tag("Courses")],
            Summary = "Chi tiết khóa học",
            Description = "Lấy khóa học theo ID.",
            Parameters = [Path("id", "ID khóa học.")],
            Responses = Responses("200", "Tìm thấy khóa học.", "404", "Không tìm thấy khóa học.")
        }));

        AddOperation(doc, "/api/courses", OperationType.Post, Protected(new OpenApiOperation
        {
            Tags = [Tag("Courses")],
            Summary = "Tạo khóa học",
            Description = "Chỉ role `Admin` được tạo khóa học.",
            RequestBody = JsonBody(ObjectSchema(("courseName", "string"), ("semesterId", "integer"), ("subjectId", "integer"))),
            Responses = Responses("201", "Tạo khóa học thành công.", "403", "Không có quyền Admin.")
        }));

        AddOperation(doc, "/api/courses/{id}", OperationType.Put, Protected(new OpenApiOperation
        {
            Tags = [Tag("Courses")],
            Summary = "Cập nhật khóa học",
            Description = "Chỉ role `Admin` được cập nhật khóa học.",
            Parameters = [Path("id", "ID khóa học.")],
            RequestBody = JsonBody(ObjectSchema(("courseName", "string"), ("semesterId", "integer"), ("subjectId", "integer"))),
            Responses = Responses("200", "Cập nhật thành công.", "404", "Không tìm thấy khóa học.")
        }));

        AddOperation(doc, "/api/courses/{id}", OperationType.Delete, Protected(new OpenApiOperation
        {
            Tags = [Tag("Courses")],
            Summary = "Xóa khóa học",
            Description = "Chỉ role `Admin` được xóa khóa học.",
            Parameters = [Path("id", "ID khóa học.")],
            Responses = Responses("200", "Xóa thành công.", "403", "Không có quyền Admin.")
        }));

        AddOperation(doc, "/api/courses/{id}/enroll", OperationType.Post, Protected(new OpenApiOperation
        {
            Tags = [Tag("Courses")],
            Summary = "Ghi danh sinh viên vào khóa học",
            Description = "Business flow chính của Lab 3. Course Service gọi gRPC sang Student Service để kiểm tra sinh viên tồn tại trước khi tạo Enrollment.",
            Parameters = [Path("id", "ID khóa học.")],
            RequestBody = JsonBody(ObjectSchema(("studentId", "integer"))),
            Responses = Responses("201", "Ghi danh thành công.", "400", "Sinh viên đã ghi danh trước đó.", "404", "Không tìm thấy khóa học hoặc sinh viên.")
        }));

        AddOperation(doc, "/api/courses/{id}/enrollments", OperationType.Get, Protected(new OpenApiOperation
        {
            Tags = [Tag("Courses")],
            Summary = "Danh sách ghi danh của khóa học",
            Description = "Lấy danh sách Enrollment thuộc một khóa học.",
            Parameters = [Path("id", "ID khóa học."), Query("page", "Số trang.", "integer"), Query("size", "Số bản ghi mỗi trang.", "integer")],
            Responses = Responses("200", "Lấy danh sách thành công.")
        }));
    }

    private static void AddSubjectsAndSemesters(OpenApiDocument doc)
    {
        AddOperation(doc, "/api/v1/subjects", OperationType.Get, Protected(new OpenApiOperation
        {
            Tags = [Tag("Catalog")],
            Summary = "Danh sách môn học",
            Description = "Lấy danh sách môn học có phân trang.",
            Parameters = [Query("page", "Số trang.", "integer"), Query("size", "Số bản ghi mỗi trang.", "integer")],
            Responses = Responses("200", "Lấy danh sách thành công.")
        }));

        AddOperation(doc, "/api/v1/semesters", OperationType.Get, Protected(new OpenApiOperation
        {
            Tags = [Tag("Catalog")],
            Summary = "Danh sách học kỳ",
            Description = "Lấy danh sách học kỳ có phân trang.",
            Parameters = [Query("page", "Số trang.", "integer"), Query("size", "Số bản ghi mỗi trang.", "integer")],
            Responses = Responses("200", "Lấy danh sách thành công.")
        }));
    }

    private static void AddEnrollments(OpenApiDocument doc)
    {
        AddOperation(doc, "/api/v1/enrollments", OperationType.Post, Protected(new OpenApiOperation
        {
            Tags = [Tag("Enrollments")],
            Summary = "Tạo ghi danh theo body",
            Description = "Route tương thích checklist Lab 2. Body chứa cả `studentId` và `courseId`; service vẫn kiểm tra sinh viên bằng gRPC trước khi ghi danh.",
            RequestBody = JsonBody(ObjectSchema(("studentId", "integer"), ("courseId", "integer"))),
            Responses = Responses("201", "Ghi danh thành công.", "400", "Sinh viên đã ghi danh trước đó.", "404", "Không tìm thấy khóa học hoặc sinh viên.")
        }));
    }

    private static void AddOperation(OpenApiDocument doc, string path, OperationType method, OpenApiOperation operation)
    {
        if (!doc.Paths.TryGetValue(path, out var pathItem))
        {
            pathItem = new OpenApiPathItem();
            doc.Paths[path] = pathItem;
        }

        pathItem.Operations[method] = operation;
    }

    private static OpenApiOperation Protected(OpenApiOperation operation)
    {
        operation.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = []
            }
        ];

        return operation;
    }

    private static OpenApiTag Tag(string name) => new() { Name = name };

    private static OpenApiParameter Query(string name, string description, string type = "string")
        => Parameter(name, ParameterLocation.Query, description, type, false);

    private static OpenApiParameter Path(string name, string description)
        => Parameter(name, ParameterLocation.Path, description, "integer", true);

    private static OpenApiParameter Parameter(string name, ParameterLocation location, string description, string type, bool required)
        => new()
        {
            Name = name,
            In = location,
            Required = required,
            Description = description,
            Schema = new OpenApiSchema { Type = type }
        };

    private static OpenApiRequestBody JsonBody(OpenApiSchema schema)
        => new()
        {
            Required = true,
            Content =
            {
                ["application/json"] = new OpenApiMediaType
                {
                    Schema = schema
                }
            }
        };

    private static OpenApiSchema ObjectSchema(params (string Name, string Type)[] properties)
    {
        var schema = new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>()
        };

        foreach (var property in properties)
        {
            schema.Properties[property.Name] = new OpenApiSchema
            {
                Type = property.Type,
                Example = ExampleFor(property.Name, property.Type)
            };
        }

        return schema;
    }

    private static IOpenApiAny ExampleFor(string name, string type)
        => type switch
        {
            "integer" => new OpenApiInteger(name.Contains("course", StringComparison.OrdinalIgnoreCase) ? 1 : 2),
            _ when name.Equals("username", StringComparison.OrdinalIgnoreCase) => new OpenApiString("admin"),
            _ when name.Equals("password", StringComparison.OrdinalIgnoreCase) => new OpenApiString("123456"),
            _ when name.Equals("email", StringComparison.OrdinalIgnoreCase) => new OpenApiString("student@lms.edu.vn"),
            _ when name.Equals("role", StringComparison.OrdinalIgnoreCase) => new OpenApiString("Student"),
            _ when name.Equals("fullName", StringComparison.OrdinalIgnoreCase) => new OpenApiString("Nguyen Van A"),
            _ when name.Equals("studentCode", StringComparison.OrdinalIgnoreCase) => new OpenApiString("SE190001"),
            _ when name.Equals("dateOfBirth", StringComparison.OrdinalIgnoreCase) => new OpenApiString("2002-01-01"),
            _ when name.Equals("courseName", StringComparison.OrdinalIgnoreCase) => new OpenApiString("PRN232 Class 1"),
            _ => new OpenApiString("")
        };

    private static OpenApiResponses Responses(params string[] pairs)
    {
        var responses = new OpenApiResponses();

        for (var i = 0; i < pairs.Length; i += 2)
        {
            var statusCode = pairs[i];
            var description = i + 1 < pairs.Length ? pairs[i + 1] : "Response";
            responses[statusCode] = new OpenApiResponse { Description = description };
        }

        return responses;
    }
}
