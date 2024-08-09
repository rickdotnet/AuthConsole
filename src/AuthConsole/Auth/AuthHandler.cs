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
            var keyPair = KeyPair.FromSeed(authConfig.SigningKey);
            var userResult = await userStore.GetUserAsync(
                authRequest.NatsConnectOptions.Username,
                authRequest.NatsConnectOptions.Password,
                cancellationToken);

            var response = new NatsAuthorizationResponseClaims
            {
                Audience = authRequest.NatsServer.Id,
                Subject = authRequest.UserNKey,
                AuthorizationResponse = new NatsAuthorizationResponse()
            };

            if (userResult.Success)
            {
                var jwtClaims = new NatsUserClaims
                {
                    Name = authRequest.NatsConnectOptions.Username,
                    Subject = authRequest.UserNKey,
                    Audience = userResult.Account!,
                    User = userResult.User!
                };

                response.AuthorizationResponse.Jwt = jwtUtil.EncodeUserClaims(jwtClaims, keyPair, DateTimeOffset.UtcNow);
                
                Log.Debug("User Jwt: {Jwt}", response.AuthorizationResponse.Jwt);
            }
            else
            {
                response.AuthorizationResponse.Error = userResult.Error ?? "Error in AuthHandler";
            }

            return response;
        }
    }
}