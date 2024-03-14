using FluentValidation;
using TaskManagerApi.DTOs;

namespace TaskManagerApi.Validations
{
	public class CreateAssignmentDTOValidator : AbstractValidator<CreateAssignmentDTO>
	{
		public CreateAssignmentDTOValidator()
		{
			RuleFor(x => x.assignment_title_nv).NotEmpty().MaximumLength(100);
			RuleFor(x => x.assignment_description_nv).MaximumLength(500);
		}
	}
}
