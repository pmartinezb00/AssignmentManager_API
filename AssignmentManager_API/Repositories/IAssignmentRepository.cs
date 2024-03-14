using TaskManagerApi.Entities;

namespace TaskManagerApi.Repositories
{
	public interface IAssignmentRepository
	{
		Task<Assignment?> GetAssignment(int idAssignment);
		Task<List<Assignment>> GetAssignmentList();
		Task<bool> CreateAssignment(Assignment assignment);
		Task<bool> FinishAssignment(int idAssignment);
		Task<bool> ReorderAssignment(Assignment assignment);
	}
}