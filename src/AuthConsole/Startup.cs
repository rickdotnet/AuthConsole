using AuthConsole.Auth;
using AuthConsole.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NATS.Extensions.Microsoft.DependencyInjection;
using NATS.Jwt;
using Serilog;
using Serilog.Events;

namespace AuthConsole;

public static class Setup
{
    internal static IHost ConfigureApplication(this HostApplicationBuilder builder)
    {
        // add default configuration
        builder.AddConfig();
        builder.AddLogging();
        builder.ConfigureNats();

        // two ways to customize the Auth service
        // either by providing your own IUserStore implementation
        builder.Services.AddSingleton<IUserStore, UserStore>();
        
        // or by replacing the AuthHandler with your own implementation
        builder.Services.AddSingleton<IAuthHandler, AuthHandler>();
        
        // this is responsible for calling the AuthHandler
        builder.Services.AddHostedService<AuthBackgroundService>();
        return builder.Build();
    }

    private static void AddConfig(this HostApplicationBuilder builder)
    {
        builder.Configuration.AddEnvironmentVariables("AUTH_");
        builder.Configuration.AddJsonFile("authConfig.json", optional: true);
        builder.Services.Configure<AuthConfig>(builder.Configuration);
        builder.Services.AddTransient(x => x.GetRequiredService<IOptions<AuthConfig>>().Value);
    }

    private static void AddLogging(this HostApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                LogEventLevel.Debug,
                "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
            .CreateLogger();

        builder.Services.AddSerilog();
    }

    private static void ConfigureNats(this HostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<NatsJwt>();
        builder.Services.AddNatsClient(
            nats => nats.ConfigureOptions(opts => opts with
            {
                Url = builder.Configuration[nameof(AuthConfig.NatsUrl)] ?? opts.Url,
                AuthOpts = opts.AuthOpts with
                {
                    Username = builder.Configuration[nameof(AuthConfig.Username)] ?? opts.AuthOpts.Username,
                    Password = builder.Configuration[nameof(AuthConfig.Password)] ?? opts.AuthOpts.Password
                }
            })
        );
    }
}