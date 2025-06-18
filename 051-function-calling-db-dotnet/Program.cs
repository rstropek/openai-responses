using FunctionCallingBasics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using System.ClientModel;
using System.Text.Json;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var client = new OpenAIResponseClient("gpt-4.1", config["OPENAI_API_KEY"]);
var systemPrompt = await File.ReadAllTextAsync("system-prompt.md");

using var sqlConnection = new SqlConnection(config["ADVENTURE_WORKS"]);
await sqlConnection.OpenAsync();

Console.WriteLine("🤖: How can I help?");

string? previousResponseId = null;

while (true)
{
string[] options =
    [
        "I will visit Orlando Gee tomorrow. Give me a revenue breakdown of his revenue per product (absolute revenue and percentages). Also show me his total revenue.",
        "Now show me a table with his revenue per year and month.",
        "The table is missing some months. Probably because they did not buy anything in those months. Complete the table by adding 0 revenue for all missing months.",
        "Show me the data in a table. Include not just percentage values, but also absolute revenue"
    ];
    Console.WriteLine("\n");
    for (int i = 0; i < options.Length; i++)
    {
        Console.WriteLine($"{i + 1}: {options[i]}");
    }

    Console.Write("You: ");
    var userMessage = Console.ReadLine();
    if (string.IsNullOrEmpty(userMessage)) { break; }
    if (int.TryParse(userMessage, out int selection) && selection >= 1 && selection <= options.Length)
    {
        userMessage = options[selection - 1];
    }
    
    Console.Write("\n🤖: ");
    previousResponseId = await client.CreateAssistantResponseAsync(sqlConnection, userMessage, systemPrompt, previousResponseId);
    Console.WriteLine();
}

public static class OpenAIResponseClientExtensions
{
    extension(OpenAIResponseClient client)
    {
        public async Task<string> CreateAssistantResponseAsync(
            SqlConnection sqlConnection,
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
                    Tools = { DatabaseFunctions.GetCustomersTool, DatabaseFunctions.GetProductsTool, DatabaseFunctions.GetCustomerProductsRevenueTool },
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
                        var jsonOptions = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                        Console.WriteLine($"\t{functionCallItem.CallId}: Calling {functionCallItem.FunctionName} with {functionCallItem.FunctionArguments}");
                        switch (functionCallItem.FunctionName)
                        {
                            case nameof(DatabaseFunctions.GetCustomers):
                                {
                                    var parameters = functionCallItem.FunctionArguments.ToObjectFromJson<GetCustomersParameters>(jsonOptions);
                                    if (parameters is null)
                                    {
                                        error = "Missing or invalid parameters";
                                        break;
                                    }

                                    var dbResult = await DatabaseFunctions.GetCustomers(sqlConnection, parameters);
                                    result = JsonSerializer.Serialize(dbResult, jsonOptions);
                                    break;
                                }
                            case nameof(DatabaseFunctions.GetProducts):
                                {
                                    var parameters = functionCallItem.FunctionArguments.ToObjectFromJson<GetProductsParameters>(jsonOptions);
                                    if (parameters is null)
                                    {
                                        error = "Missing or invalid parameters";
                                        break;
                                    }

                                    var dbResult = await DatabaseFunctions.GetProducts(sqlConnection, parameters);
                                    result = JsonSerializer.Serialize(dbResult, jsonOptions);
                                    break;
                                }
                            case nameof(DatabaseFunctions.GetCustomerProductsRevenue):
                                {
                                    var parameters = functionCallItem.FunctionArguments.ToObjectFromJson<GetCustomerProductsRevenueParameters>(jsonOptions);
                                    if (parameters is null)
                                    {
                                        error = "Missing or invalid parameters";
                                        break;
                                    }

                                    var dbResult = await DatabaseFunctions.GetCustomerProductsRevenue(sqlConnection, parameters);
                                    result = JsonSerializer.Serialize(dbResult, jsonOptions);
                                    break;
                                }
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