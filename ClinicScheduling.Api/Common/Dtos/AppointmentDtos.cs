using System.ComponentModel.DataAnnotations;

namespace ClinicScheduling.Api.Common.Dtos;

public static class AppointmentDtos
{
    public class Create
    {
        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Range(1, 480)]
        public int DurationMinutes { get; set; }

        [MaxLength(300)]
        public string? Reason { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Scheduled";

        [MaxLength(300)]
        public string? CancellationReason { get; set; }
    }

    public class Update
    {
        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        [Required]
        public DateTime EndDateTime { get; set; }

        [Range(1, 480)]
        public int DurationMinutes { get; set; }

        [MaxLength(300)]
        public string? Reason { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = null!;

        [MaxLength(300)]
        public string? CancellationReason { get; set; }

        public bool IsActive { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = null!;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = null!;
        public string? CancellationReason { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class DoctorBusySlotResponse
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class AppointmentAviableDoctorDto
    {
        public Guid DoctorId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DurationMinutes { get; set; }
    }
}
