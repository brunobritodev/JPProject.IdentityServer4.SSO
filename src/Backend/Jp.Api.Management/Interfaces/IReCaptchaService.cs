using System.Threading.Tasks;

namespace Jp.Api.Management.Interfaces
{
    public interface IReCaptchaService
    {
        Task<bool> IsCaptchaPassed();
        Task<bool> IsCaptchaEnabled();
    }
}