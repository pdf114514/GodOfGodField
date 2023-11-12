using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class ApplicationState {
    private readonly IJSRuntime JS;
    private readonly NavigationManager Navigation;

    public bool IsLoginScreen => Navigation.Uri == Navigation.BaseUri;
    public bool IsPlaying { get; set; } = false;

    public string UserName { get; set; } = string.Empty;

    public string IdToken { get; set; } = string.Empty;
    public string LocalId { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } = -1;
    public string RefreshToken { get; set; } = string.Empty;

    public string SessionId { get; set; } = string.Empty;
    public string GSessionId { get; set; } = string.Empty;

    public ApplicationState(IJSRuntime js, NavigationManager navigation) {
        JS = js;
        Navigation = navigation;
    }

    public async Task Load() {
        UserName = await JS.LSGetItem("UserName") ?? string.Empty;
        IdToken = await JS.LSGetItem("IdToken") ?? string.Empty;
        LocalId = await JS.LSGetItem("LocalId") ?? string.Empty;
        ExpiresIn = int.Parse(await JS.LSGetItem("ExpiresIn") ?? "-1");
        RefreshToken = await JS.LSGetItem("RefreshToken") ?? string.Empty;
    }

    public async Task Save() {
        await JS.LSSetItem("UserName", UserName);
        await JS.LSSetItem("IdToken", IdToken);
        await JS.LSSetItem("LocalId", LocalId);
        await JS.LSSetItem("ExpiresIn", ExpiresIn.ToString());
        await JS.LSSetItem("RefreshToken", RefreshToken);
    }
}