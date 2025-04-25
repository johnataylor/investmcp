using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuickstartWeatherServer.Tools;
using System.Net.Http.Headers;

var builder = Host.CreateApplicationBuilder(args);

using (StreamWriter sw = File.AppendText("C:\\private\\mcp\\investmcp\\Looping\\QuickstartWeatherServer\\mylog.txt"))
{
    await sw.WriteLineAsync($"**** STARTED ****");
}

//builder.Services.AddMcpServer()
//    .WithStdioServerTransport()
//    .WithTools<WeatherTools>();

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<WeatherTools>()
    .WithTools<NicknameTools>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton(_ =>
{
    var client = new HttpClient() { BaseAddress = new Uri("https://api.weather.gov") };
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("weather-tool", "1.0"));
    return client;
});

await builder.Build().RunAsync();
