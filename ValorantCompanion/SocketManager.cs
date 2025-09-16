using RadiantConnect;
using RadiantConnect.SocketServices.InternalTcp;
using System.Text.Json;

public class SocketManager
{
    private readonly Initiator _initiator;
    private ValSocket _socket;

    // Event that anyone can subscribe to
    public event Action<string>? OnMessageReceived;

    public SocketManager(Initiator initiator)
    {
        _initiator = initiator;
    }

    public void Initialize()
    {
        _socket = new ValSocket(_initiator);
        _socket.InitializeConnection();
        Console.WriteLine("Initializing socket");

        _socket.OnNewMessage += HandleNewMessage;
    }

    private void HandleNewMessage(string data)
    {
        try
        {
            using var doc = JsonDocument.Parse(data);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 3)
                return;

            var dataElement = root[2].GetProperty("data");
            if (!dataElement.TryGetProperty("payload", out var payloadProp))
                return;

            string? payloadStr = payloadProp.GetString();
            if (string.IsNullOrEmpty(payloadStr))
                return;

            using var payloadDoc = JsonDocument.Parse(payloadStr);
            var payloadRoot = payloadDoc.RootElement;

            if (!payloadRoot.TryGetProperty("loopState", out var loopStateProp))
                return;

            string loopState = loopStateProp.GetString() ?? "";

            // Notify all subscribers
            OnMessageReceived?.Invoke(loopState);
        }
        catch
        {
            // Ignore bad messages
        }
    }
}