using System.ComponentModel.DataAnnotations;

namespace LMS.Course.Application.DTOs;

public class CourseDTO
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public int SemesterId { get; set; }
    public int SubjectId { get; set; }
}

public class CreateCourseRequest
{
    [Required]
    [StringLength(100)]
    public string CourseName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int SemesterId { get; set; }

    [Range(1, int.MaxValue)]
    public int SubjectId { get; set; }
}

public class SemesterDTO
{
    public int SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class SubjectDTO
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Credit { get; set; }
}

public class EnrollmentDTO
{
    public int EnrollmentId { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateEnrollmentRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }
}

public class CreateEnrollmentDirectRequest
{
    [Range(1, int.MaxValue)]
    public int StudentId { get; set; }

    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }
}
