using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.OutputCaching;
using TaskManagerApi.DTOs;
using TaskManagerApi.Entities;
using TaskManagerApi.Repositories;

namespace TaskManagerApi.Endpoints
{
	public static class AssignmentEndpoint
	{
		#region CTO
		public static RouteGroupBuilder MapAssignments(this RouteGroupBuilder group)
		{
			group.MapGet("/get/{idAssignment:int}", GetAssignment).CacheOutput(c => c.Expire(TimeSpan.FromMinutes(20)).Tag("assignment"));
			group.MapGet("/list", GetAssignmentList).CacheOutput(c => c.Expire(TimeSpan.FromMinutes(20)).Tag("assignment-list"));
			group.MapPost("/create", CreateAssignment);
			group.MapPut("/finish/{idAssignment:int}", FinishAssignment);
			group.MapPut("/reorder", ReorderAssignment);

			return group;
		}
		#endregion

		static async Task<Results<Ok<AssignmentDTO>, NotFound>> GetAssignment(int idAssignment, IAssignmentRepository _repo, IMapper _mapper)
		{
			var assignment = _mapper.Map<AssignmentDTO>(await _repo.GetAssignment(idAssignment));
			if (assignment is null) { return TypedResults.NotFound(); }
			return TypedResults.Ok(assignment);
		}

		static async Task<Ok<List<AssignmentDTO>>> GetAssignmentList(IAssignmentRepository _repo, IMapper _mapper)
		{
			var assignment = _mapper.Map<List<AssignmentDTO>>(await _repo.GetAssignmentList());
			return TypedResults.Ok(assignment);
		}

		static async Task<Results<Created, ValidationProblem>> CreateAssignment(CreateAssignmentDTO assignment, IAssignmentRepository _repo, IOutputCacheStore _outputCache, IMapper _mapper, IValidator<CreateAssignmentDTO> _validator)
		{
			var resultValidation = await _validator.ValidateAsync(assignment);
			if (!resultValidation.IsValid) { return TypedResults.ValidationProblem(resultValidation.ToDictionary()); }

			await _repo.CreateAssignment(_mapper.Map<Assignment>(assignment));
			await _outputCache.EvictByTagAsync("assignment", default);
			await _outputCache.EvictByTagAsync("assignment-list", default);
			return TypedResults.Created();
		}

		static async Task<Results<NoContent, NotFound>> FinishAssignment(int idAssignment, IAssignmentRepository _repo, IOutputCacheStore _outputCache)
		{
			var result = await _repo.FinishAssignment(idAssignment);
			if (!result) { return TypedResults.NotFound(); }
			await _outputCache.EvictByTagAsync("assignment-list", default);
			return TypedResults.NoContent();
		}

		static async Task<Results<NoContent, NotFound, ValidationProblem>> ReorderAssignment(ReorderAssignmentDTO assignment, IAssignmentRepository _repo, IOutputCacheStore _outputCache, IMapper _mapper, IValidator<ReorderAssignmentDTO> _validator)
		{
			var resultValidation = await _validator.ValidateAsync(assignment);
			if (!resultValidation.IsValid) { return TypedResults.ValidationProblem(resultValidation.ToDictionary()); }

			var result = await _repo.ReorderAssignment(_mapper.Map<Assignment>(assignment));
			if (!result) { return TypedResults.NotFound(); }
			await _outputCache.EvictByTagAsync("assignment", default);
			await _outputCache.EvictByTagAsync("assignment-list", default);
			return TypedResults.NoContent();
		}
	}
}
