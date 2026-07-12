namespace LMS.Course.Domain.Entities;

public class Enrollment
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public CourseEntity Course { get; set; } = null!;
}
