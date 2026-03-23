using AutoMapper;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;

namespace ClinicScheduling.Api.Common.MapperProfiles;

public class AppointmentProfileMapper:Profile
{
    public AppointmentProfileMapper()
    {
        CreateMap<AppointmentEntity, AppointmentDtos.Response>()
            .ForMember(dest => dest.DoctorName,
                opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Name : string.Empty))
            .ForMember(dest => dest.PatientName,
                opt => opt.MapFrom(src => src.Patient != null ? src.Patient.Name : string.Empty));

        CreateMap<AppointmentDtos.Create, AppointmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<AppointmentDtos.Update, AppointmentEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Doctor, opt => opt.Ignore())
            .ForMember(dest => dest.Patient, opt => opt.Ignore());
    }
}