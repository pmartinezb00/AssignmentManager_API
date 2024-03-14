using Dapper;
using Microsoft.Data.SqlClient;
using TaskManagerApi.Entities;

namespace TaskManagerApi.Repositories
{
	public class AssignmentRepository : IAssignmentRepository
	{
		#region CTO
		private readonly string connectionString;

		public AssignmentRepository(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection")!;
		}
		#endregion

		public async Task<Assignment?> GetAssignment(int idAssignment)
		{
			using (var connection = new SqlConnection(connectionString))
			{
				var query = @"SELECT * FROM Assignment WHERE assignment_id_i = @idAssignment";
				var assignment = await connection.QueryFirstOrDefaultAsync<Assignment>(query, new { idAssignment });
				return assignment;
			}
		}

		public async Task<List<Assignment>> GetAssignmentList()
		{
			using (var connection = new SqlConnection(connectionString))
			{
				var query = @"SELECT * FROM Assignment";
				var listAssignments = await connection.QueryAsync<Assignment>(query);
				return listAssignments.ToList();
			}
		}

		public async Task<bool> CreateAssignment(Assignment assignment)
		{
			var result = false;

			using (var connection = new SqlConnection(connectionString))
			{
				var query = @"INSERT INTO Assignment (
							  	    assignment_title_nv
							  	  , assignment_description_nv
							  	  , assignment_order_i
							  	  , assignment_done_b
							  )
							  SELECT @assignment_title_nv
							  	   , @assignment_description_nv
							  	   , COALESCE(MAX(assignment_order_i), 0) + 1
							  	   , 0
							  FROM Assignment";

				result = await connection.ExecuteAsync(query, assignment) > 0;
			}

			return result;
		}
	
		public async Task<bool> FinishAssignment(int idAssignment)
		{
			var result = false;

			using (var connection = new SqlConnection(connectionString))
			{
				var query = @"IF NOT EXISTS (SELECT TOP 1 1 FROM Assignment WHERE assignment_id_i = @idAssignment AND assignment_done_b = 0)
							  BEGIN
							  	  RETURN;
							  END

							  DECLARE @assignment_order_i INT
							  SELECT @assignment_order_i = assignment_order_i FROM Assignment WHERE assignment_id_i = @idAssignment

							  UPDATE Assignment SET 
								    assignment_order_i = assignment_order_i - 1
							  WHERE assignment_id_i != @idAssignment
								  AND COALESCE(assignment_order_i, -1) > @assignment_order_i

							  UPDATE Assignment SET 
								    assignment_order_i = NULL
								  , assignment_done_b = 1 
							  WHERE assignment_id_i = @idAssignment;";

				result = await connection.ExecuteAsync(query, new { idAssignment }) > 0;
			}

			return result;
		}

		public async Task<bool> ReorderAssignment(Assignment assignment)
		{
			var result = false;

			using (var connection = new SqlConnection(connectionString))
			{
				var query = @"IF NOT EXISTS (SELECT TOP 1 1 FROM Assignment WHERE assignment_id_i = @assignment_id_i AND assignment_done_b = 0)
							  BEGIN
							      RETURN;
							  END
							  
							  IF NOT EXISTS (SELECT TOP 1 1 FROM Assignment WHERE COALESCE(assignment_order_i, -1) = @assignment_order_i)
							  BEGIN
							      SELECT @assignment_order_i = MAX(assignment_order_i)
								  FROM Assignment
								  WHERE assignment_done_b = 0
							  END

							  IF (@assignment_order_i = (SELECT MAX(assignment_order_i)
															FROM Assignment
															WHERE assignment_done_b = 0))
							  BEGIN
								  SET @assignment_order_i = @assignment_order_i + 1;
							  END

							  DROP TABLE IF EXISTS #reorder_assignments

							  SELECT assignment_id_i
								   , assignment_order_i
						      INTO #reorder_assignments
							  FROM Assignment 
							  WHERE assignment_done_b = 0

							  UPDATE #reorder_assignments SET
								  assignment_order_i = assignment_order_i + 1
							  WHERE assignment_id_i != @assignment_id_i
								  AND assignment_order_i >= @assignment_order_i

							  UPDATE #reorder_assignments SET
								  assignment_order_i = @assignment_order_i
							  WHERE assignment_id_i = @assignment_id_i

							  ;WITH cte_reorder AS (
								  SELECT assignment_id_i, ROW_NUMBER() OVER (ORDER BY assignment_order_i) new_order
								  FROM #reorder_assignments
							  )
							  UPDATE T_UPDATE SET
								  assignment_order_i = new_order
							  FROM cte_reorder cte
							  INNER JOIN Assignment T_UPDATE ON T_UPDATE.assignment_id_i = cte.assignment_id_i

							  DROP TABLE IF EXISTS #reorder_assignments;";

				result = await connection.ExecuteAsync(query, assignment) > 0;
			}

			return result;
		}
	}
}
