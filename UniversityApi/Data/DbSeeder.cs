using UniversityApi.Models;

namespace UniversityApi.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        if (db.Teachers.Any() || db.Students.Any() || db.Courses.Any())
            return;

        var now = DateTime.UtcNow;

        var teachers = new[]
        {
            new Teacher { FirstName = "Aigerim", LastName = "Nurlanova", Email = "a.nurlanova@satbayev.university", Department = "Software Engineering" },
            new Teacher { FirstName = "Yevgeniy", LastName = "Gertsen", Email = "e.gertsen@satbayev.university", Department = "Software Engineering" },
            new Teacher { FirstName = "Marat", LastName = "Serikov", Email = "m.serikov@satbayev.university", Department = "Mathematics" }
        };
        db.Teachers.AddRange(teachers);
        await db.SaveChangesAsync();

        var students = new[]
        {
            new Student { FirstName = "Alex", LastName = "Smith", Email = "alex.smith@example.com", BirthDate = new DateOnly(2004, 3, 14), CreatedAt = now },
            new Student { FirstName = "Dana", LastName = "Kim", Email = "dana.kim@example.com", BirthDate = new DateOnly(2003, 11, 2), CreatedAt = now },
            new Student { FirstName = "Ruslan", LastName = "Akhmetov", Email = "ruslan.akhmetov@example.com", BirthDate = new DateOnly(2004, 7, 21), CreatedAt = now },
            new Student { FirstName = "Aliya", LastName = "Bekova", Email = "aliya.bekova@example.com", BirthDate = new DateOnly(2005, 1, 9), CreatedAt = now },
            new Student { FirstName = "Timur", LastName = "Omarov", Email = "timur.omarov@example.com", BirthDate = new DateOnly(2003, 5, 30), CreatedAt = now }
        };
        db.Students.AddRange(students);
        await db.SaveChangesAsync();

        var courses = new[]
        {
            new Course { Name = "Web Services Development", Description = "REST API on ASP.NET Core", Credits = 5, TeacherId = teachers[1].Id, CreatedAt = now },
            new Course { Name = "Databases", Description = "Relational databases and SQL", Credits = 4, TeacherId = teachers[0].Id, CreatedAt = now },
            new Course { Name = "Discrete Mathematics", Description = "Sets, graphs, combinatorics", Credits = 3, TeacherId = teachers[2].Id, CreatedAt = now },
            new Course { Name = "Software Architecture", Description = "Layers, patterns, DI", Credits = 5, TeacherId = teachers[0].Id, CreatedAt = now }
        };
        db.Courses.AddRange(courses);
        await db.SaveChangesAsync();

        db.Enrollments.AddRange(
            new Enrollment { StudentId = students[0].Id, CourseId = courses[0].Id, EnrollmentDate = now, Grade = 92.5m },
            new Enrollment { StudentId = students[0].Id, CourseId = courses[1].Id, EnrollmentDate = now, Grade = 85m },
            new Enrollment { StudentId = students[1].Id, CourseId = courses[0].Id, EnrollmentDate = now },
            new Enrollment { StudentId = students[2].Id, CourseId = courses[2].Id, EnrollmentDate = now, Grade = 70m },
            new Enrollment { StudentId = students[3].Id, CourseId = courses[3].Id, EnrollmentDate = now },
            new Enrollment { StudentId = students[4].Id, CourseId = courses[1].Id, EnrollmentDate = now, Grade = 88m });
        await db.SaveChangesAsync();
    }
}
