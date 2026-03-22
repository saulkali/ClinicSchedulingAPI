using System.ComponentModel.DataAnnotations;

namespace ClinicScheduling.Api.Common.Dtos;

public static class DoctorScheduleDtos
{
    public class Create
    {
        [Required]
        public Guid DoctorId { get; set; }

        [Range(1, 7)]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }
    }

    public class Update
    {
        [Required]
        public Guid DoctorId { get; set; }

        [Range(1, 7)]
        public int DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = null!;
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
