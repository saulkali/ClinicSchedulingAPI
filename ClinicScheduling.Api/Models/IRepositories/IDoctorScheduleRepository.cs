using ClinicScheduling.Api.Common.Database.Entities;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface IDoctorScheduleRepository
{
    Task<IEnumerable<DoctorScheduleEntity>> GetAllAsync();
    Task<DoctorScheduleEntity?> GetByIdAsync(Guid id);
    Task<DoctorScheduleEntity> CreateAsync(DoctorScheduleEntity entity);
    Task<DoctorScheduleEntity?> UpdateAsync(DoctorScheduleEntity entity);
    Task<bool> DeleteAsync(Guid id);
}
