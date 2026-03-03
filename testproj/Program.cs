using System;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
#pragma warning disable OPENAI001
using OpenAI.Assistants;


class Program {
    static async Task Main() {
        var credential = new System.ClientModel.ApiKeyCredential("E5mcwV1ZDxmI5GnoLHEw6Ep3atiSf6BA7orOowXtZ2uv88GTS3oyJQQJ99BKACYeBjFXJ3w3AAAAACOG3i6K");
        var clientOptions = new AzureOpenAIClientOptions(AzureOpenAIClientOptions.ServiceVersion.V2024_10_01_Preview); // Try preview API version
        var client = new AzureOpenAIClient(new Uri("https://duymai-ai-foundry.cognitiveservices.azure.com/"), credential, clientOptions);
        
        var assistantClient = client.GetAssistantClient();
        
        var options = new AssistantCreationOptions {
            Name = "Student Helper",
            Instructions = "You are a helpful AI Assistant for students."
        };
        
        try {
            var assistantResult = await assistantClient.CreateAssistantAsync("gpt-4o", options);
            var assistant = assistantResult.Value;
            Console.WriteLine($"Created Assistant ID: {assistant.Id}");
            
            var threadResult = await assistantClient.CreateThreadAsync();
            var thread = threadResult.Value;
            Console.WriteLine($"Created Thread ID: {thread.Id}");
            
            await assistantClient.CreateMessageAsync(thread.Id, MessageRole.User, new [] { MessageContent.FromText("Hello!") });
            
            var stream = assistantClient.CreateRunStreamingAsync(thread.Id, assistant.Id);
            await foreach (var update in stream) {
                if (update is MessageContentUpdate contentUpdate) {
                    Console.Write(contentUpdate.Text);
                }
            }
        } catch (Exception ex) {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
