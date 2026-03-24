using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ClinicScheduling.Api.Common.Enums;

namespace ClinicScheduling.Api.Common.Database.Entities;

[Table("Appointments")]
public class AppointmentEntity:BaseEntity
{
    [Required]
    public Guid DoctorId { get; set; }

    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public DateTime StartDateTime { get; set; }

    [Required]
    public DateTime EndDateTime { get; set; }

    [Required]
    public int DurationMinutes { get; set; }

    [MaxLength(300)]
    public string? Reason { get; set; }

    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = nameof(AppointmentStatus.Scheduled);

    [MaxLength(300)]
    public string? CancellationReason { get; set; }

    [ForeignKey(nameof(DoctorId))]
    public DoctorEntity Doctor { get; set; } = null!;

    [ForeignKey(nameof(PatientId))]
    public PatientEntity Patient { get; set; } = null!;
}