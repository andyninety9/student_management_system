using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using SmsRazor.BLL.Services;

namespace SmsRazor.BLL.Hubs;

[Authorize]
public class AssistantHub : Hub
{
    private readonly IAssistantService _assistantService;
    private readonly IConfiguration _configuration;

    public AssistantHub(IAssistantService assistantService, IConfiguration configuration)
    {
        _assistantService = assistantService;
        _configuration = configuration;
    }

    public async Task SendMessageToAssistant(string message)
    {
        try
        {
            var modelName = _configuration["AIFoundry:ModelName"] ?? "gpt-4o";
            var studentCode = Context.User?.FindFirst("StudentCode")?.Value;
            
            // Generate a unique ID for this message response
            var responseId = Guid.NewGuid().ToString();
            
            // Notify client that we are starting the stream
            await Clients.Caller.SendAsync("ReceiveAssistantStreamStart", responseId);

            var stream = _assistantService.GetStreamingChatMessageAsync(message, Context.ConnectionId, studentCode, modelName);

            await foreach (var contentChunk in stream)
            {
                await Clients.Caller.SendAsync("ReceiveAssistantStreamChunk", responseId, contentChunk);
            }

            // Notify client that stream has finished
            await Clients.Caller.SendAsync("ReceiveAssistantStreamEnd", responseId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error streaming AI Assistant message: {ex.Message}");
            await Clients.Caller.SendAsync("ReceiveAssistantError", "An error occurred while generating a response.");
        }
    }
}
