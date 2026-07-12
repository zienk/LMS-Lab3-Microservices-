namespace LMS.Course.Domain.Entities;

public class CourseEntity
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }
    
    public Semester Semester { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
}
