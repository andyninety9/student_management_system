using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace SmsRazor.BLL.Services;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string containerName);
    Task<bool> DeleteFileAsync(string fileUrl, string containerName);
}
