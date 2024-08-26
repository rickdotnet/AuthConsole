using NATS.NKeys;

namespace AuthConsole;

public class Tool
{
    public static void RunTool()
    {
        var keyPair = KeyPair.CreatePair(PrefixByte.Account);
        var seed = keyPair.GetSeed();
        var publicKey = keyPair.GetPublicKey();
        Console.WriteLine($"Seed {seed}");
        Console.WriteLine($"Public Key: {publicKey}");

        // Convert string message to byte array  
        var message = "password"u8.ToArray();
        var messageMemory = new ReadOnlyMemory<byte>(message);

        // Sign the message  
        var signature = new Memory<byte>(new byte[64]);
        keyPair.Sign(messageMemory, signature);

        // Convert signature to Base64 string  
        var signatureString = Convert.ToBase64String(signature.ToArray());
        Console.WriteLine($"Signature: {signatureString}");

        var sigBytes = Convert.FromBase64String(signatureString);
        var key2 = KeyPair.FromPublicKey(publicKey);
        var valid = key2.Verify(messageMemory, sigBytes);
        Console.WriteLine($"Valid: {valid}");

        return;
    }
}