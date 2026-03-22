using ClinicScheduling.Api.Common.Database.Entities;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface ISpecialtyRepository
{
    Task<IEnumerable<SpecialtyEntity>> GetAllAsync();
    Task<SpecialtyEntity?> GetByIdAsync(Guid id);
    Task<SpecialtyEntity> CreateAsync(SpecialtyEntity entity);
    Task<SpecialtyEntity?> UpdateAsync(SpecialtyEntity entity);
    Task<bool> DeleteAsync(Guid id);
}
