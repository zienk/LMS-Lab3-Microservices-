namespace LMS.Student.Domain.Entities;

public class Student
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
