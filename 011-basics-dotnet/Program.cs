using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var client = new OpenAIResponseClient("gpt-4.1", config["OPENAI_API_KEY"]);
var systemPrompt = await File.ReadAllTextAsync("system-prompt.md");

var lastAssistantMessage = "How can I help you?";
List<ResponseItem> messages = [
  ResponseItem.CreateAssistantMessageItem(systemPrompt),
  ResponseItem.CreateAssistantMessageItem(lastAssistantMessage),
];

while (true) {
  Console.WriteLine($"\n🤖: {lastAssistantMessage}");

  // get user input
  Console.Write("\nYou (empty to quit): ");
  var userMessage = Console.ReadLine()!;
  if (string.IsNullOrEmpty(userMessage)) {
    break;
  }

  messages.Add(ResponseItem.CreateUserMessageItem(userMessage));

  var response = await client.CreateResponseAsync(messages);

  messages.Add(ResponseItem.CreateAssistantMessageItem(response.Value.GetOutputText()));
  lastAssistantMessage = response.Value.GetOutputText();
}

