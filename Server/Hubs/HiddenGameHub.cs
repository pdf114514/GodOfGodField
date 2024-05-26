using Microsoft.AspNetCore.SignalR;

namespace GodOfGodField.Server.Hubs;

public class HiddenGameHub : Hub {
    internal static readonly List<HiddenGameRoom> Rooms = [];
    
    public const string HubUrl = "/hiddenGame";

    public HiddenGameHub() => Rooms.Add(new HiddenGameRoom { Id = "DEBUG" });

    public override Task OnConnectedAsync() {
        Console.WriteLine($"Connected {Context.ConnectionId}");
        Rooms[0].Clients.Add(Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? e) {
        Console.WriteLine($"Disconnected {Context.ConnectionId}");
        return base.OnDisconnectedAsync(e);
    }

    public void BroadCast(string room, string message) {
        Clients.Group(room).SendAsync("ReceiveMessage", message);
    }
}

public class HiddenGameRoom {
    public required string Id { get; set; }
    public List<string> Clients { get; set; } = [];
}