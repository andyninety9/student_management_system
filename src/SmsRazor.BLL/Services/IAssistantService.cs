using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SmsRazor.BLL.Services;

public interface IAssistantService
{
    IAsyncEnumerable<string> GetStreamingChatMessageAsync(string message, string connectionId, string? studentCode, string modelName, CancellationToken cancellationToken = default);
}
