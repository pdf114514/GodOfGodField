using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class ApplicationState {
    private readonly IJSRuntime JS;

    public string UserName { get; set; } = string.Empty;
    public bool IsLoginScreen { get; set; } = true;
    public bool IsPlaying { get; set; } = false;

    public ApplicationState(IJSRuntime js) => JS = js;

    public async Task Load() {
        UserName = await JS.LSGetItem("UserName") ?? string.Empty;
    }

    public void Save() {
        JS.LSSetItem("UserName", UserName);
    }
}