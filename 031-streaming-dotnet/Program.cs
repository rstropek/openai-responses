using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

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

    Console.Write("\n🤖: ");
    previousResponseId = await client.CreateAndPrintResponse(userMessage, systemPrompt, previousResponseId);
    Console.WriteLine();
}

public static class OpenAIResponseClientExtensions
{
    extension(OpenAIResponseClient client)
    {
        public async Task<string> CreateAndPrintResponse(
            string userMessage, 
            string systemPrompt, 
            string? previousResponseId)
        {
            var response = client.CreateResponseStreamingAsync(userMessage, new()
            {
                PreviousResponseId = previousResponseId,
                Instructions = systemPrompt,
                StoredOutputEnabled = true,
            });
            await foreach (var chunk in response)
            {
                switch (chunk)
                {
                    case StreamingResponseCreatedUpdate create:
                        previousResponseId = create.Response.Id;
                        break;
                    case StreamingResponseOutputTextDeltaUpdate textDelta:
                        Console.Write(textDelta.Delta);
                        break;
                }
            }

            return previousResponseId!;
        }
        
    }
}