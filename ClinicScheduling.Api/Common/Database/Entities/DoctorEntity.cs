using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicScheduling.Api.Common.Database.Entities;

[Table("Doctors")]
public class DoctorEntity:BaseEntity
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid SpecialtyId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = null!;

    [MaxLength(20)]
    public string? Phone { get; set; }

    // Navigation Properties

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;

    [ForeignKey(nameof(SpecialtyId))]
    public SpecialtyEntity Specialty { get; set; } = null!;

    public ICollection<DoctorScheduleEntity> Schedules { get; set; } = new List<DoctorScheduleEntity>();

    public ICollection<AppointmentEntity> Appointments { get; set; } = new List<AppointmentEntity>();
}