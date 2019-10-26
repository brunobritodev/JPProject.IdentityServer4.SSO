using AutoMapper;
using Jp.Application.EventSourcedNormalizers;
using Jp.Application.ViewModels.RoleViewModels;
using Jp.Application.ViewModels.UserViewModels;
using Jp.Domain.Core.Events;
using Jp.Domain.Models;
using System.Globalization;

namespace Jp.Application.AutoMapper
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            CreateMap<User, UserViewModel>(MemberList.Destination);//.ForMember(x => x.LockoutEnd, opt => opt.MapFrom(src => src.LockoutEnd != null ? src.LockoutEnd.Value.DateTime.ToShortDateString() : string.Empty));
            CreateMap<User, UserListViewModel>(MemberList.Destination);

            CreateMap<StoredEvent, EventHistoryData>().ConstructUsing(a => new EventHistoryData(a.Message, a.Id.ToString(), a.Details, a.Timestamp.ToString(CultureInfo.InvariantCulture), a.User, a.MessageType, a.RemoteIpAddress));

            CreateMap<Role, RoleViewModel>(MemberList.Destination);
            CreateMap<UserLogin, UserLoginViewModel>(MemberList.Destination);

        }
    }
}
