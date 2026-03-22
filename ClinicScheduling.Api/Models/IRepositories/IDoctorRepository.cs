using ClinicScheduling.Api.Common.Database.Entities;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface IDoctorRepository
{
    Task<IEnumerable<DoctorEntity>> GetAllAsync();
    Task<DoctorEntity?> GetByIdAsync(Guid id);
    Task<DoctorEntity> CreateAsync(DoctorEntity entity);
    Task<DoctorEntity?> UpdateAsync(DoctorEntity entity);
    Task<bool> DeleteAsync(Guid id);
}
