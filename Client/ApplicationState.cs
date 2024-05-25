using System.Text.Json;
using GodOfGodField.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class ApplicationState(IJSInProcessRuntime js, NavigationManager navigation) {
    public bool IsLoginScreen => navigation.Uri == navigation.BaseUri;
    public bool IsPlaying => navigation.Uri.EndsWith("/game");

    public string UserName { get => js.LSGetItem(nameof(UserName)) ?? string.Empty; set => js.LSSetItem(nameof(UserName), value); }
    public string RoomPassword { get => js.LSGetItem(nameof(RoomPassword)) ?? string.Empty; set => js.LSSetItem(nameof(RoomPassword), value); }

    public string IdToken { get => js.LSGetItem(nameof(IdToken)) ?? string.Empty; set => js.LSSetItem(nameof(IdToken), value); }
    public string LocalId { get => js.LSGetItem(nameof(LocalId)) ?? string.Empty; set => js.LSSetItem(nameof(LocalId), value); }
    public int ExpiresIn { get => int.Parse(js.LSGetItem(nameof(ExpiresIn)) ?? "-1"); set => js.LSSetItem(nameof(ExpiresIn), value.ToString()); }
    public string RefreshToken { get => js.LSGetItem(nameof(RefreshToken)) ?? string.Empty; set => js.LSSetItem(nameof(RefreshToken), value); }

    public GFSession Session { get; set; } = new() { GSessionId = string.Empty, SessionId = string.Empty };
    public string RoomId { get; set; } = string.Empty;

    public JsonDocument? HiddenRoomDocument { get; set; }
}