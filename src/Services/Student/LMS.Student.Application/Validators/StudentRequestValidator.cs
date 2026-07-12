using FluentValidation;
using LMS.Student.Application.DTOs;

namespace LMS.Student.Application.Validators;

public class StudentRequestValidator : AbstractValidator<StudentRequest>
{
    public StudentRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.StudentCode)
            .NotEmpty().WithMessage("Student code is required.")
            .Matches(@"^(SE|CE|SA|QE)\d{6}$")
            .WithMessage("Student code must follow FPTU format (e.g. SE193418).");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today.AddYears(-15))
            .WithMessage("Student must be at least 15 years old.");
    }
}
