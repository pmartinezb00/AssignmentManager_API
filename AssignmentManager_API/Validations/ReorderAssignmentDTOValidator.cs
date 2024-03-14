using FluentValidation;
using TaskManagerApi.DTOs;

namespace TaskManagerApi.Validations
{
	public class ReorderAssignmentDTOValidator : AbstractValidator<ReorderAssignmentDTO>
	{
		public ReorderAssignmentDTOValidator()
		{
			RuleFor(x => x.assignment_order_i).GreaterThan(0);
		}
	}
}
