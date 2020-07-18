using JPProject.Sso.Application.ViewModels.UserViewModels;

namespace Jp.UI.SSO.Interfaces
{
    public interface IAuthService
    {
        UserViewModel Login(string username, string password);
    }
}