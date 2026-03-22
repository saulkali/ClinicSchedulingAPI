using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicScheduling.Api.Common.Database.Entities;

[Table("DoctorSchedules")]
public class DoctorScheduleEntity:BaseEntity
{

    [Required]
    public Guid DoctorId { get; set; }

    /// <summary>
    /// 1 = Monday ... 7 = Sunday
    /// </summary>
    [Required]
    [Range(1,7)]
    public int DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    // Navigation
    [ForeignKey(nameof(DoctorId))]
    public DoctorEntity Doctor { get; set; } = null!;
}