using ClinicScheduling.Api.Common.Database.Entities;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface IPatientRepository
{
    Task<IEnumerable<PatientEntity>> GetAllAsync();
    Task<PatientEntity?> GetByIdAsync(Guid id);
    Task<PatientEntity> CreateAsync(PatientEntity entity);
    Task<PatientEntity?> UpdateAsync(PatientEntity entity);
    Task<bool> DeleteAsync(Guid id);
}
