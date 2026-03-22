namespace ClinicScheduling.Api.Common.Dtos;

public static class RoleDtos
{
    public class Create
    {
        public string Name { get; set; } = null!;
    }

    public class Update
    {
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
    }

    public class Response
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }
}