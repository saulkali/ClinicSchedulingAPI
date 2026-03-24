using ClinicScheduling.Api.Common.Database.Entities;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface IAppointmentRepository
{
    Task<IEnumerable<AppointmentEntity>> GetAllAsync();
    Task<AppointmentEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<AppointmentEntity>> GetByPatientIdAsync(Guid patientId);
    Task<AppointmentEntity> CreateAsync(AppointmentEntity entity);
    Task<AppointmentEntity?> UpdateAsync(AppointmentEntity entity);
    Task<bool> DeleteAsync(Guid id);
}
