using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using System.ClientModel;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var client = new OpenAIResponseClient("gpt-4.1", config["OPENAI_API_KEY"]);
var systemPrompt = await File.ReadAllTextAsync("system-prompt.md");

Console.WriteLine("🤖: How can I help?");

string? previousResponseId = null;

while (true)
{
    Console.Write("\nYou (empty to quit): ");
    var userMessage = Console.ReadLine()!;
    if (string.IsNullOrEmpty(userMessage))
    {
        break;
    }

    var response = await client.CreateAssistantResponseAsync(userMessage, systemPrompt, previousResponseId);
    
    Console.WriteLine($"\n🤖: {response.Value.GetOutputText()}");
    previousResponseId = response.Value.Id;
}

public static class OpenAIResponseClientExtensions
{
    extension(OpenAIResponseClient client)
    {
        public async Task<ClientResult<OpenAIResponse>> CreateAssistantResponseAsync(
            string userMessage, 
            string systemPrompt, 
            string? previousResponseId)
        {
            return await client.CreateResponseAsync(userMessage, new()
            {
                PreviousResponseId = previousResponseId,
                Instructions = systemPrompt,
                StoredOutputEnabled = true,
            });
        }
        
    }
}