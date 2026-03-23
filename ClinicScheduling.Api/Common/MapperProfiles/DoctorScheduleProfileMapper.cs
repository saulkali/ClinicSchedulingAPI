using AutoMapper;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;

namespace ClinicScheduling.Api.Common.MapperProfiles;

public class DoctorScheduleProfileMapper : Profile
{
    public DoctorScheduleProfileMapper()
    {
        CreateMap<DoctorScheduleEntity, DoctorScheduleDtos.Response>()
            .ForMember(dest => dest.DoctorName,
                opt => opt.MapFrom(src => src.Doctor != null ? src.Doctor.Name : string.Empty));

        CreateMap<DoctorScheduleDtos.Create, DoctorScheduleEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<DoctorScheduleDtos.Update, DoctorScheduleEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Doctor, opt => opt.Ignore());
    }
}
