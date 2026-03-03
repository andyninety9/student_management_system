using System.Threading.Tasks;
using SmsRazor.BLL.DTOs;

namespace SmsRazor.BLL.Services;

public interface IAccountService
{
    Task InitializeSystemAsync();
    Task<bool> IsEmailRegisteredAsync(string email);
    Task RegisterAdminAsync(string email, string password, string fullName);
    Task<LoginResult> LoginAsync(string email, string password);
}
