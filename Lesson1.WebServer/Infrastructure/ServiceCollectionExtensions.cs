using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lesson1.WebServer.Data;
using Lesson1.WebServer.Endpoints;
using Lesson1.WebServer.Services;
using Microsoft.EntityFrameworkCore;

namespace Lesson1.WebServer.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLessonCoreServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddProblemDetails();
        services.AddRazorPages();
        
        // Настройка JSON сериализации с игнорированием циклических ссылок
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        });
        
        services.AddSingleton(TimeProvider.System);
        services.Configure<ServerInfoOptions>(configuration.GetSection(ServerInfoOptions.SectionName));
        services.AddSingleton<IServerInfoProvider, ServerInfoProvider>();
        services.AddSingleton<IMathService, MathService>();
        services.AddSingleton<IEchoService, EchoService>();

        services.AddDbContext<TaskDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITaskStore, EfTaskStore>();

        return services;
    }

    public static IServiceCollection AddLessonEndpointGroups(this IServiceCollection services)
    {
        services.AddSingleton<IEndpointGroup, HelloEndpointGroup>();
        services.AddSingleton<IEndpointGroup, ServerInfoEndpointGroup>();
        services.AddSingleton<IEndpointGroup, ArithmeticEndpointGroup>();
        services.AddSingleton<IEndpointGroup, EchoEndpointGroup>();
        services.AddSingleton<IEndpointGroup, TaskEndpointGroup>();

        return services;
    }
}
