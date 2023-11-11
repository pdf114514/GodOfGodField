using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class ApplicationState {
    public string UserName { get; set; } = string.Empty;
    public bool IsLoginScreen { get; set; } = true;
    public bool IsPlaying { get; set; } = false;
}