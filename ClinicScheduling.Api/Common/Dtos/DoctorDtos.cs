namespace ClinicScheduling.Api.Common.Dtos;

public static class DoctorDtos
{
    public class Create
    {
        public Guid UserId { get; set; }
        public Guid SpecialtyId { get; set; }
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
    }

    public class Update
    {
        public Guid SpecialtyId { get; set; }
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}