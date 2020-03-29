using Api.ManagerGift.DTO;
using Api.ManagerGift.Entities;
using AutoMapper;

namespace Api.ManagerGift.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDTO>();
            CreateMap<UserDTO, User>();

            CreateMap<Organization, OrganizationDTO>();
            CreateMap<Organization, OrganizationDetailDTO>();
            CreateMap<Organization, NewOrganizationDTO>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.text, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ManageCode, opt => opt.MapFrom(src => src.ManageCode))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId));
            CreateMap<OrganizationDTO, Organization>();
        }
    }
}
