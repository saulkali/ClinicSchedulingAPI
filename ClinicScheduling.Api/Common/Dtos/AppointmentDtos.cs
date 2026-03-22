using System.ComponentModel.DataAnnotations;

namespace ClinicScheduling.Api.Common.Dtos;

public static class AppointmentDtos
{
    /// <summary>
    /// Solicitud para crear una cita médica.
    /// </summary>
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

    /// <summary>
    /// Solicitud para actualizar una cita médica.
    /// </summary>
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

    /// <summary>
    /// Respuesta de una cita médica con alertas y metadatos de agenda.
    /// </summary>
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
        public bool HasCancellationAlert { get; set; }
        public int RecentCancellationCount { get; set; }
        public IReadOnlyCollection<DateTime> SuggestedSlots { get; set; } = Array.Empty<DateTime>();
    }
}
