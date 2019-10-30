using AutoMapper;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using JPProject.Sso.Infra.Identity.Models.Identity;

namespace Jp.Management.Configuration
{
    internal class CustomMappingProfile : Profile
    {
        public CustomMappingProfile()
        {

            CreateMap<UserIdentity, UserViewModel>();
        }
    }
}