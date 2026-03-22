namespace ClinicScheduling.Api.Common.Dtos;

public static class PatientDtos
{
    public class Create
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public string? Phone { get; set; }
    }

    public class Update
    {
        public string Name { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = null!;
        public DateTime? BirthDate { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}