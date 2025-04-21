using Anthropic.SDK;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

var (command, arguments) = GetCommandAndArguments(args);

var clientTransport = new StdioClientTransport(new()
{
    Name = "Demo Server",
    Command = command,
    Arguments = arguments,
});

await using var mcpClient = await McpClientFactory.CreateAsync(clientTransport);

var tools = await mcpClient.ListToolsAsync();
foreach (var tool in tools)
{
    Console.WriteLine($"Connected to server with tools: {tool.Name}");
}

using var chatClient = new AnthropicClient(new APIAuthentication(builder.Configuration["ANTHROPIC_API_KEY"]))
    .Messages
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

var options = new ChatOptions
{
    MaxOutputTokens = 1000,
    ModelId = "claude-3-5-sonnet-20241022",
    Tools = [.. tools]
};

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("MCP Client Started!");
Console.ResetColor();

//await RunChatLoopNoHistory(chatClient, options);
await RunChatLoop(chatClient, options);

static async Task RunChatLoop(IChatClient chatClient, ChatOptions options, bool history = true)
{
    List<ChatMessage> chatHistory = [];

    PromptForInput();
    while (Console.ReadLine() is string query && !"exit".Equals(query, StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            PromptForInput();
            continue;
        }

        if (history == false)
        {
            chatHistory.Clear();
        }

        chatHistory.Add(new ChatMessage(ChatRole.User, query));

        var response = string.Empty;
        await foreach (var message in chatClient.GetStreamingResponseAsync(chatHistory, options))
        {
            Console.Write(message.Text);
            response += message.Text;
        }
        Console.WriteLine();

        chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));

        PromptForInput();
    }
}


static void PromptForInput()
{
    //Console.WriteLine("Enter a command (or 'exit' to quit):");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("> ");
    Console.ResetColor();
}

/// <summary>
/// Determines the command (executable) to run and the script/path to pass to it. This allows different
/// languages/runtime environments to be used as the MCP server.
/// </summary>
/// <remarks>
/// This method uses the file extension of the first argument to determine the command, if it's py, it'll run python,
/// if it's js, it'll run node, if it's a directory or a csproj file, it'll run dotnet.
/// 
/// If no arguments are provided, it defaults to running the QuickstartWeatherServer project from the current repo.
/// 
/// This method would only be required if you're creating a generic client, such as we use for the quickstart.
/// </remarks>
static (string command, string[] arguments) GetCommandAndArguments(string[] args)
{
    //return args switch
    //{
    //    [var script] when script.EndsWith(".py") => ("python", args),
    //    [var script] when script.EndsWith(".js") => ("node", args),
    //    [var script] when Directory.Exists(script) || (File.Exists(script) && script.EndsWith(".csproj")) => ("dotnet", ["run", "--project", script, "--no-build"]),
    //    _ => ("dotnet", ["run", "--project", "../../../../QuickstartWeatherServer", "--no-build"])
    //};

    return args switch
    {
        [var script] when script.EndsWith(".py") => ("python", args),
        [var script] when script.EndsWith(".js") => ("node", args),
        [var script] when Directory.Exists(script) || (File.Exists(script) && script.EndsWith(".csproj")) => ("dotnet", ["run", "--project", script, "--no-build"]),
        //_ => ("dotnet", ["run", "--project", "../QuickstartWeatherServer", "--no-build"])
        _ => ("dotnet", ["run", "--project", "../QuickstartWeatherServer"])
    };
}