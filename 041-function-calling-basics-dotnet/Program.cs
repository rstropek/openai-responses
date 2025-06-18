using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using FunctionCallingBasics;
using System.Text.Json;

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
    previousResponseId = await client.CreateAssistantResponseAsync(userMessage, systemPrompt, previousResponseId);
    Console.WriteLine();
}

public static class OpenAIResponseClientExtensions
{
    extension(OpenAIResponseClient client)
    {
        public async Task<string> CreateAssistantResponseAsync(
            string userMessage,
            string systemPrompt,
            string? previousResponseId)
        {
            List<ResponseItem> input = [
              ResponseItem.CreateUserMessageItem(userMessage),
            ];

            while (true)
            {
                var response = await client.CreateResponseAsync(input, new()
                {
                    Instructions = systemPrompt,
                    ToolChoice = ResponseToolChoice.CreateAutoChoice(),
                    Tools = { PasswordFunctions.BuildPasswordTool, },
                    StoredOutputEnabled = true,
                    PreviousResponseId = previousResponseId,
                });

                input.Clear();

                foreach (var item in response.Value.OutputItems)
                {
                    if (item is FunctionCallResponseItem functionCallItem)
                    {
                        var result = "";
                        string? error = null;

                        Console.WriteLine($"\t{functionCallItem.CallId}: Calling {functionCallItem.FunctionName} with {functionCallItem.FunctionArguments}");
                        switch (functionCallItem.FunctionName)
                        {
                            case nameof(PasswordFunctions.BuildPasswordTool):
                                var parameters = functionCallItem.FunctionArguments.ToObjectFromJson<BuildPasswordParameters>(new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                                if (parameters is null)
                                {
                                    error = "Missing or invalid parameters";
                                    break;
                                }

                                result = PasswordFunctions.BuildPassword(parameters);
                                input.Add(ResponseItem.CreateFunctionCallOutputItem(functionCallItem.CallId, result));
                                break;
                            default:
                                error = $"Unknown function: {functionCallItem.FunctionName}";
                                break;
                        }

                        input.Add(ResponseItem.CreateFunctionCallOutputItem(functionCallItem.CallId, error ?? result));
                        previousResponseId = response.Value.Id;
                    }
                    else if (item is MessageResponseItem messageItem)
                    {
                        Console.WriteLine(messageItem.Content[0].Text);
                        return response.Value.Id;
                    }
                }
            }
        }
    }
}