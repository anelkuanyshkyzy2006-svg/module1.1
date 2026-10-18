using AutoMapper;
using UniversityApi.DTO.Courses;
using UniversityApi.DTO.Enrollments;
using UniversityApi.DTO.Students;
using UniversityApi.DTO.Teachers;
using UniversityApi.Models;

namespace UniversityApi.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Students
        CreateMap<StudentCreateDto, Student>();
        CreateMap<StudentUpdateDto, Student>();
        CreateMap<Student, StudentDto>();
        CreateMap<Enrollment, StudentCourseDto>()
            .ForMember(d => d.EnrollmentId, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name))
            .ForMember(d => d.Credits, o => o.MapFrom(s => s.Course.Credits));
        CreateMap<Student, StudentWithCoursesDto>()
            .ForMember(d => d.Courses, o => o.MapFrom(s => s.Enrollments));

        // Teachers
        CreateMap<TeacherCreateDto, Teacher>();
        CreateMap<TeacherUpdateDto, Teacher>();
        CreateMap<Teacher, TeacherDto>();

        // Courses
        CreateMap<CourseCreateDto, Course>();
        CreateMap<CourseUpdateDto, Course>();
        CreateMap<Course, CourseDto>()
            .ForMember(d => d.TeacherName, o => o.MapFrom(s =>
                s.Teacher != null ? s.Teacher.FirstName + " " + s.Teacher.LastName : null));

        // Enrollments
        CreateMap<EnrollmentCreateDto, Enrollment>();
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(d => d.StudentName, o => o.MapFrom(s => s.Student.FirstName + " " + s.Student.LastName))
            .ForMember(d => d.CourseName, o => o.MapFrom(s => s.Course.Name));
    }
}
