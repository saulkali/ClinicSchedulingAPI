using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicScheduling.Api.Common.Database.Entities;

[Table("Users")]
public class UserEntity:BaseEntity
{
    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    public Guid RoleId { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(RoleId))]
    public RoleEntity Role { get; set; } = null!;

    public DoctorEntity? Doctor { get; set; }
    public PatientEntity? Patient { get; set; }
    
    public bool VerifyPassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
    }
}