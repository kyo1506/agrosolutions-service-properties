using AgroSolutions.Properties.Domain.Entities;
using AgroSolutions.Properties.Shared.DTOs;
using AutoMapper;

namespace AgroSolutions.Properties.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Produtor, ProdutorDto>();
        CreateMap<Fazenda, FazendaDto>();
        CreateMap<Talhao, TalhaoDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<Sensor, SensorDto>()
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
