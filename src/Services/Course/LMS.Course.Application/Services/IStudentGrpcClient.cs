namespace LMS.Course.Application.Services;

public interface IStudentGrpcClient
{
    Task<bool> CheckStudentExistsAsync(int studentId);
}
