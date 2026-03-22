using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicScheduling.Api.Common.Database.Entities;

[Table("Specialties")]
public class SpecialtyEntity:BaseEntity
{

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    // Duration of appointment based on specialty (minutes)
    [Required]
    [Range(5, 240)]
    public int AppointmentDurationMinutes { get; set; }

    // Navigation
    public ICollection<DoctorEntity> Doctors { get; set; } = new List<DoctorEntity>();
}