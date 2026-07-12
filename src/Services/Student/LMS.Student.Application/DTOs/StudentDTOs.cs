using System.ComponentModel.DataAnnotations;

namespace LMS.Student.Application.DTOs;

public class StudentRequest
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^(SE|CE|SA|QE)\d{6}$")]
    [StringLength(20)]
    public string StudentCode { get; set; } = string.Empty;

    [Required]
    public DateTime DateOfBirth { get; set; }
}

public class StudentResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
}
