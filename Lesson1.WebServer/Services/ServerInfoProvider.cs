using Lesson1.WebServer.Contracts;
using Microsoft.Extensions.Options;

namespace Lesson1.WebServer.Services;

public sealed class ServerInfoProvider(
    IOptions<ServerInfoOptions> options,
    TimeProvider timeProvider) : IServerInfoProvider
{
    public ServerInfoResponse GetInfo()
    {
        var settings = options.Value;
        return new ServerInfoResponse(
            settings.ServerName,
            settings.Version,
            timeProvider.GetLocalNow().DateTime);
    }
}
