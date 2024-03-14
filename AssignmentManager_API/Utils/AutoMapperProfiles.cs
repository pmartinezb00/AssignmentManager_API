using AutoMapper;
using TaskManagerApi.DTOs;
using TaskManagerApi.Entities;

namespace TaskManagerApi.Utils
{
	public class AutoMapperProfiles : Profile
	{
		public AutoMapperProfiles()
		{
			CreateMap<CreateAssignmentDTO, Assignment>();
			CreateMap<ReorderAssignmentDTO, Assignment>();
			
			CreateMap<Assignment, AssignmentDTO>();
		}
	}
}
