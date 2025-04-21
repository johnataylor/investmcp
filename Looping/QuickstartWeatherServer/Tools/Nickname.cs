using ModelContextProtocol.Server;
using System.ComponentModel;

namespace QuickstartWeatherServer.Tools;

[McpServerToolType]
public sealed class NicknameTools
{
    [McpServerTool, Description("Retrieves a place name given a nickname.")]
    public static async Task<string> GetPlaceByNickname(
        [Description("The nickname of the place.")] string nickname)
    {
        var result = nickname switch
        {
            var s when s.Contains("emerald city", StringComparison.InvariantCultureIgnoreCase) => "Seattle",
            var s when s.Contains("big apple", StringComparison.InvariantCultureIgnoreCase) => "New York",
            var s when s.Contains("city of angels", StringComparison.InvariantCultureIgnoreCase) => "Los Angels",
            var s when s.Contains("city of light", StringComparison.InvariantCultureIgnoreCase) => "Paris",
            var s when s.Contains("eternal city", StringComparison.InvariantCultureIgnoreCase) => "Rome",
            _ => "sorry I have no idea"
        };

        using (StreamWriter sw = File.AppendText(".\\mylog.txt"))
        {
            await sw.WriteLineAsync($"GetPlaceByNickname ( {nickname} ) => {result}");
        }

        return result;
    }
}
