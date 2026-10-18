using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Models;

public class Student : IEntity
{
    // Primary Key — по соглашению (Convention): свойство "Id"
    public int Id { get; set; }

    // Data Annotations
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
