using System.ComponentModel.DataAnnotations;

namespace ClinicScheduling.Api.Common.Dtos;

public static class PatientDtos
{
    public class Create
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        public DateTime? BirthDate { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }
    }

    public class Update
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        public DateTime? BirthDate { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public bool IsActive { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = null!;
        public string Name { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
