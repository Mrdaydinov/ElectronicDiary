using AutoMapper;
using ElectronicDiary.Domain.Entities;
using ElectronicDiary.WebApi.DTOs;
using ElectronicDiary.WebAPI.DTOs;

namespace ElectronicDiary.WebApi.Helpers.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Student mappings
            CreateMap<Student, StudentDto>().ReverseMap();

            // Teacher mappings
            CreateMap<Teacher, TeacherDto>().ReverseMap();

            // Assignment mappings
            CreateMap<Assignment, AssignmentDto>().ReverseMap();

            // Grade mappings
            CreateMap<Grade, GradeDto>().ReverseMap();
        }
    }
}
