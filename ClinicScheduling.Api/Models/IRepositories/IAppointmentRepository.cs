using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;

namespace ClinicScheduling.Api.Models.IRepositories;

public interface IAppointmentRepository
{
    Task<IEnumerable<AppointmentEntity>> GetAllAsync();
    Task<AppointmentEntity?> GetByIdAsync(Guid id);
    Task<IEnumerable<AppointmentEntity>> GetByPatientIdAsync(Guid patientId);
    Task<IEnumerable<AppointmentEntity>> GetByDoctorIdAsync(Guid doctorId);
    Task<AppointmentDtos.AppointmentAviableDoctorDto> GetDoctorAvailabilityAsync(Guid doctorId, DateTime date);
    Task<AppointmentEntity> CreateAsync(AppointmentEntity entity);
    Task<AppointmentEntity?> UpdateAsync(AppointmentEntity entity);
    Task<bool> DeleteAsync(Guid id);
}
