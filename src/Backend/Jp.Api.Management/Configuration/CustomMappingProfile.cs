using AutoMapper;
using JPProject.Sso.Application.ViewModels.UserViewModels;
using JPProject.Sso.AspNetIdentity.Models.Identity;

namespace Jp.Api.Management.Configuration
{
    internal class CustomMappingProfile : Profile
    {
        public CustomMappingProfile()
        {

            CreateMap<UserIdentity, UserViewModel>();
        }
    }
}