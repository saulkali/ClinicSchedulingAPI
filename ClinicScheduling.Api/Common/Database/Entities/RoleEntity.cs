using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicScheduling.Api.Common.Database.Entities;

[Table("Roles")]
public class RoleEntity:BaseEntity
{


    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = null!;

    // Navigation
    public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
}