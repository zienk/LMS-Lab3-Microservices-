using LMS.Course.Application.Services;
using LMS.Student.Grpc;
using Microsoft.Extensions.Logging;

namespace LMS.Course.GrpcClient.Services;

public class StudentGrpcClientImpl : IStudentGrpcClient
{
    private readonly StudentGrpcService.StudentGrpcServiceClient _client;
    private readonly ILogger<StudentGrpcClientImpl> _logger;

    public StudentGrpcClientImpl(StudentGrpcService.StudentGrpcServiceClient client, ILogger<StudentGrpcClientImpl> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<bool> CheckStudentExistsAsync(int studentId)
    {
        try
        {
            _logger.LogInformation("Calling gRPC CheckStudentExists for ID: {Id}", studentId);
            var request = new CheckStudentExistsRequest { StudentId = studentId.ToString() };
            var response = await _client.CheckStudentExistsAsync(request);
            return response.Exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Student gRPC service for ID: {Id}", studentId);
            throw; // Or return false based on business requirement
        }
    }
}
