using NATS.Jwt.Models;

namespace AuthConsole.Auth;

public interface IUserStore
{
    ValueTask<NatsUserResult> GetUserAsync(string user, string password, CancellationToken cancellationToken);
}
public record NatsUserResult
{
    public bool Success { get; init; }
    public NatsUser? User { get; init; }
    public string? Error { get; init; } 
}