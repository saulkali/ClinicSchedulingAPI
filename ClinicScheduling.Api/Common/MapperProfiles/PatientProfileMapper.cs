using AutoMapper;
using ClinicScheduling.Api.Common.Database.Entities;
using ClinicScheduling.Api.Common.Dtos;

namespace ClinicScheduling.Api.Common.MapperProfiles;

public class PatientProfileMapper : Profile
{
    public PatientProfileMapper()
    {
        CreateMap<PatientEntity, PatientDtos.Response>()
            .ForMember(dest => dest.UserEmail,
                opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));

        CreateMap<PatientDtos.Create, PatientEntity>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(_ => true));

        CreateMap<PatientDtos.Update, PatientEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Appointments, opt => opt.Ignore());
    }
}
