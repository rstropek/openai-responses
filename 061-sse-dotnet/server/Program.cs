using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Responses;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["OPENAI_API_KEY"];
    return new OpenAIResponseClient("gpt-4.1", apiKey);
});
builder.Services.AddSingleton(new State(await File.ReadAllTextAsync("system-prompt.md"), []));
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.MapPost("/messages", ([FromBody] NewUserMessage message, [FromServices] State state) =>
{
    state.MessageHistory.Add(ResponseItem.CreateUserMessageItem(message.Message));
    return TypedResults.Ok();
});

app.MapGet("/run", (OpenAIResponseClient client, [FromServices] State state, CancellationToken cancellationToken, ILogger<Program> logger) =>
{
    async IAsyncEnumerable<AssistantResponseMessage> GetAssistantStreaming([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var response = client.CreateResponseStreamingAsync(state.MessageHistory, new()
        {
            Instructions = state.SystemPrompt,
            StoredOutputEnabled = false,
        }, cancellationToken);
        var result = new StringBuilder();
        await foreach (var chunk in response)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            if (chunk is StreamingResponseOutputTextDeltaUpdate textDelta)
            {
                logger.LogInformation("Received text delta: {Delta}", textDelta.Delta);
                result.Append(textDelta.Delta);
                yield return new AssistantResponseMessage(textDelta.Delta);
            }
        }

        yield return new AssistantResponseMessage("<|DONE|>");
        state.MessageHistory.Add(ResponseItem.CreateAssistantMessageItem(result.ToString()));
    }

    return TypedResults.ServerSentEvents(GetAssistantStreaming(cancellationToken), eventType: "textDelta");
});

app.Run();

record NewUserMessage(string Message);
record AssistantResponseMessage(string DeltaText);
record State(string SystemPrompt, List<ResponseItem> MessageHistory);
