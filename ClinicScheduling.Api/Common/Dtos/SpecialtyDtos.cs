namespace ClinicScheduling.Api.Common.Dtos;

public static class SpecialtyDtos
{
    public class Create
    {
        public string Name { get; set; } = null!;
        public int AppointmentDurationMinutes { get; set; }
    }

    public class Update
    {
        public string Name { get; set; } = null!;
        public int AppointmentDurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int AppointmentDurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}