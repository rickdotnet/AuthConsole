using AuthConsole.Auth;
using NATS.Jwt.Models;

namespace AuthConsole.Stores;

public class UserStore : IUserStore
{
    public ValueTask<NatsUserResult> GetUserAsync(string user, string password, CancellationToken cancellationToken)
    {
        // simulate read-only for steve
        var readOnly = user.Equals("steve", StringComparison.OrdinalIgnoreCase);
        
        // for steve
        var noPublish = new NatsPermission { Deny = ["*"] };
        
        // everyone gets to publish to their own subjects
        var basePublish = new NatsPermission { Allow = [$"{user}.>"] };
        
        // everyone gets to subscribe to their own subjects
        var baseSubscribe = new NatsPermission { Allow = [$"{user}.>"] };

        return new ValueTask<NatsUserResult>(new NatsUserResult
        {
            Success = true,
            User = new NatsUser
            {
                Pub = readOnly ? noPublish : basePublish,
                Sub = baseSubscribe,
                Resp = new NatsResponsePermission { MaxMsgs = 1 }
            }
        });
    }
}