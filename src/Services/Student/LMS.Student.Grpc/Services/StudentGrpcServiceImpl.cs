using Grpc.Core;
using LMS.Student.Application.Services;
using Microsoft.Extensions.Logging;

namespace LMS.Student.Grpc.Services;

public class StudentGrpcServiceImpl : StudentGrpcService.StudentGrpcServiceBase
{
    private readonly IStudentService _studentService;
    private readonly ILogger<StudentGrpcServiceImpl> _logger;

    public StudentGrpcServiceImpl(IStudentService studentService, ILogger<StudentGrpcServiceImpl> logger)
    {
        _studentService = studentService;
        _logger = logger;
    }

    public override async Task<StudentReply> GetStudentById(GetStudentByIdRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetStudentById: {Id}", request.StudentId);

        if (!int.TryParse(request.StudentId, out var id))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid student ID format."));

        var student = await _studentService.GetByIdAsync(id);
        if (student is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Student {id} not found."));

        return new StudentReply
        {
            StudentId = student.StudentId.ToString(),
            FullName = student.FullName,
            Email = student.Email,
            IsActive = student.IsActive
        };
    }

    public override async Task<CheckStudentExistsReply> CheckStudentExists(CheckStudentExistsRequest request, ServerCallContext context)
    {
        _logger.LogInformation("gRPC CheckStudentExists: {Id}", request.StudentId);

        if (!int.TryParse(request.StudentId, out var id))
            return new CheckStudentExistsReply { Exists = false };

        var exists = await _studentService.ExistsAsync(id);
        return new CheckStudentExistsReply { Exists = exists };
    }
}
