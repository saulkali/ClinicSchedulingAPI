using System.ComponentModel.DataAnnotations;

namespace ClinicScheduling.Api.Common.Dtos;

public static class SpecialtyDtos
{
    public class Create
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Range(5, 240)]
        public int AppointmentDurationMinutes { get; set; }
    }

    public class Update
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Range(5, 240)]
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
