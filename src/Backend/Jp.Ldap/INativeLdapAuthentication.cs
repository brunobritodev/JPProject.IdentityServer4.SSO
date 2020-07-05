using JPProject.Sso.Application.ViewModels.UserViewModels;

namespace Jp.Ldap
{
    public interface ILdapAuthentication
    {
        UserViewModel Login(string username, string password);
    }
}