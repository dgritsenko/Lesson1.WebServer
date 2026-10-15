namespace Lesson1.WebServer.Services;

public sealed class ServerInfoOptions
{
    public const string SectionName = "ServerInfo";

    public string ServerName { get; set; } = "Lesson1.WebServer";

    public string Version { get; set; } = "1.0";
}
