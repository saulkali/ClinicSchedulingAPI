using System.ComponentModel.DataAnnotations;

namespace ClinicScheduling.Api.Common.Dtos;

public static class DoctorDtos
{
    public class Create
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
    }

    public class Update
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

        public bool IsActive { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = null!;
        public Guid SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}
