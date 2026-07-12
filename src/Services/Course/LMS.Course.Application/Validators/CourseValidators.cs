using FluentValidation;
using LMS.Course.Application.DTOs;

namespace LMS.Course.Application.Validators;

public class CreateEnrollmentRequestValidator : AbstractValidator<CreateEnrollmentRequest>
{
    public CreateEnrollmentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Student ID must be greater than 0.");
    }
}

public class CreateEnrollmentDirectRequestValidator : AbstractValidator<CreateEnrollmentDirectRequest>
{
    public CreateEnrollmentDirectRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0).WithMessage("Student ID must be greater than 0.");

        RuleFor(x => x.CourseId)
            .GreaterThan(0).WithMessage("Course ID must be greater than 0.");
    }
}

public class CreateCourseRequestValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseRequestValidator()
    {
        RuleFor(x => x.CourseName)
            .NotEmpty().WithMessage("Course name is required.")
            .MaximumLength(100);

        RuleFor(x => x.SemesterId)
            .GreaterThan(0).WithMessage("Semester ID must be greater than 0.");

        RuleFor(x => x.SubjectId)
            .GreaterThan(0).WithMessage("Subject ID must be greater than 0.");
    }
}
