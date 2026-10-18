using System.ComponentModel.DataAnnotations;

namespace UniversityApi.Models;

public class Course : IEntity
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int Credits { get; set; }

    // Foreign Key
    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
