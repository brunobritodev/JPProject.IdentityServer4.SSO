using IdentityServer4.Models;
using JPProject.Admin.Application.ViewModels.ClientsViewModels;
using JPProject.Sso.Application.ViewModels;

namespace Jp.Api.Management.ViewModel
{
    public class SaveClientWithLogoViewModel : SaveClientViewModel
    {
        public FileUploadViewModel Logo { get; set; }
    }
    public class UpdateClientWithLogoViewModel : Client
    {
        public FileUploadViewModel Logo { get; set; }
    }
}
