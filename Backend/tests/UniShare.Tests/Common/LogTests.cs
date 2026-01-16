using UniShare.Common;
using Xunit;

namespace UniShare.Tests.Common;

public class LogTests
{
    [Fact]
    public void Info_Writes_To_Log_File()
    {
        var marker = $"info-marker-{Guid.NewGuid()}";

        Log.Info(marker);

        var contents = File.ReadAllText(GetLogFilePath());
        Assert.Contains(marker, contents);
    }

    [Fact]
    public void Error_Writes_Exception_Details()
    {
        var marker = $"error-marker-{Guid.NewGuid()}";

        Log.Error(marker, new InvalidOperationException("boom"));

        var contents = File.ReadAllText(GetLogFilePath());
        Assert.Contains(marker, contents);
        Assert.Contains("InvalidOperationException", contents);
    }

    private static string GetLogFilePath()
    {
        var projectRoot = Directory.GetParent(AppContext.BaseDirectory)!
            .Parent!
            .Parent!
            .Parent!
            .FullName;

        return Path.Combine(projectRoot, "logs", "log.txt");
    }
}
