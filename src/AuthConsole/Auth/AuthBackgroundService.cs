using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NATS.Client.Core;
using NATS.Jwt;
using NATS.Jwt.Models;
using NATS.NKeys;
using Serilog;

namespace AuthConsole.Auth;

public class AuthBackgroundService : BackgroundService
{
    private const string AuthSubject = "$SYS.REQ.USER.AUTH";
    private readonly INatsConnection connection;
    private readonly IAuthHandler handler;
    private readonly AuthConfig authConfig;
    private readonly NatsJwt jwt;

    public AuthBackgroundService(IServiceProvider serviceProvider)
    {
        connection = serviceProvider.GetRequiredService<INatsConnection>();
        handler = serviceProvider.GetRequiredService<IAuthHandler>();
        authConfig = serviceProvider.GetRequiredService<AuthConfig>();
        jwt = serviceProvider.GetRequiredService<NatsJwt>();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await foreach (var msg in connection.SubscribeAsync<string>(AuthSubject, cancellationToken: cancellationToken))
        {
            Log.Information("Received Auth Request");
            Log.Debug("Received Jwt {Jwt}", msg.Data);
            
            // decode JWT
            var jwtSplit = msg.Data!.Split('.');
            var payload = EncodingUtils.FromBase64UrlEncoded(jwtSplit[1]);
            var authRequest = JsonSerializer.Deserialize<NatsAuthorizationRequestClaims>(payload);
            Log.Verbose("Authorization Request: {Request}", authRequest);

            // handle auth
            var responseClaims = await handler.HandleAsync(authRequest!.AuthorizationRequest, cancellationToken);
            var responseJwt = jwt.EncodeAuthorizationResponseClaims(responseClaims,
                KeyPair.FromSeed(authConfig.SigningKey), DateTimeOffset.UtcNow);

            Log.Debug("Response JWT: {Jwt}", responseJwt);

            var responseBytes = Encoding.UTF8.GetBytes(responseJwt);
            
            Log.Information("Replying to Auth Request");
            await msg.ReplyAsync(responseBytes, cancellationToken: cancellationToken);
        }
    }
}