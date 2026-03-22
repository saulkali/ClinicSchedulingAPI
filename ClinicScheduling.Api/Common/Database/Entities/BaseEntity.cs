using System.ComponentModel.DataAnnotations;

namespace ClinicScheduling.Api.Common.Database.Entities;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}