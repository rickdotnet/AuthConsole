using AuthConsole.Stores;
using NATS.Jwt;
using NATS.Jwt.Models;
using NATS.NKeys;
using Serilog;

namespace AuthConsole.Auth
{
    public interface IAuthHandler
    {
        ValueTask<NatsAuthorizationResponseClaims> HandleAsync(NatsAuthorizationRequest authRequest,
            CancellationToken cancellationToken);
    }

    public class AuthHandler : IAuthHandler
    {
        private readonly IUserStore userStore;
        private readonly AuthConfig authConfig;
        private readonly NatsJwt jwtUtil;

        public AuthHandler(IUserStore userStore, NatsJwt jwtUtil, AuthConfig authConfig)
        {
            this.userStore = userStore;
            this.jwtUtil = jwtUtil;
            this.authConfig = authConfig;
        }

        public async ValueTask<NatsAuthorizationResponseClaims> HandleAsync(NatsAuthorizationRequest authRequest,
            CancellationToken cancellationToken)
        {
            var kp = KeyPair.FromSeed(authConfig.SigningKey);
            var userResult = await userStore.GetUserAsync(
                authRequest.NatsConnectOptions.Username,
                authRequest.NatsConnectOptions.Password,
                cancellationToken);

            var jwtClaims = new NatsUserClaims
            {
                Audience = "APP",
                Name = authRequest.NatsConnectOptions.Username,
                Subject = authRequest.UserNKey
            };

            if (userResult.Success)
                jwtClaims.User = userResult.User!;

            var userJwt = jwtUtil.EncodeUserClaims(jwtClaims, kp, DateTimeOffset.UtcNow);
            Log.Debug("User Jwt: {Jwt}", userJwt);

            var response = new NatsAuthorizationResponseClaims
            {
                Audience = authRequest.NatsServer.Id,
                Subject = authRequest.UserNKey,
                AuthorizationResponse = new NatsAuthorizationResponse()
            };
            if (userResult.Success)
                response.AuthorizationResponse.Jwt = userJwt;
            else
                response.AuthorizationResponse.Error = userResult.Error ?? "Error in AuthHandler";

            return response;
        }
    }
}