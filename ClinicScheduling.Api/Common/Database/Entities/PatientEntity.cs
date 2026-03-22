using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicScheduling.Api.Common.Database.Entities;

[Table("Patients")]
public class PatientEntity:BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    public DateTime? BirthDate { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    // Navigation properties

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;

    public ICollection<AppointmentEntity> Appointments { get; set; } = new List<AppointmentEntity>();
}