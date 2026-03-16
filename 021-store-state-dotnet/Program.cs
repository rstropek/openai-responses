using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using System.ClientModel;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var client = new ResponsesClient(config["OPENAI_API_KEY"]);
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

public static class ResponsesClientExtensions
{
    extension(ResponsesClient client)
    {
        public async Task<ClientResult<ResponseResult>> CreateAssistantResponseAsync(
            string userMessage,
            string systemPrompt,
            string? previousResponseId)
        {
            return await client.CreateResponseAsync(new CreateResponseOptions()
            {
                Model = "gpt-5.2",
                PreviousResponseId = previousResponseId,
                Instructions = systemPrompt,
                StoredOutputEnabled = true,
                InputItems = { ResponseItem.CreateUserMessageItem(userMessage) },
            });
        }

    }
}
