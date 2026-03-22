namespace ClinicScheduling.Api.Common.Dtos;

public static class AppointmentDtos
{
    public class Create
    {
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime StartDateTime { get; set; }
        public string? Reason { get; set; }
    }

    public class Update
    {
        public DateTime StartDateTime { get; set; }
        public string? Reason { get; set; }
        public string Status { get; set; } = null!;
        public string? CancellationReason { get; set; }
        public bool IsActive { get; set; }
    }

    public class Response
    {
        public int Id { get; set; }
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
}