using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class Firebase {
    static Firebase _instance;
    readonly IJSInProcessRuntime JS;
    readonly Dictionary<string, Action<object?>> Callbacks = [];

    public Firebase(IJSInProcessRuntime js) {
        _instance = this;
        JS = js;
    }

    public ValueTask SignIn() => JS.InvokeVoidAsync("FirebaseSignIn");

    public async Task<string> Subscribe<T>(string docPath, Action<string, T?> func) where T : class {
        var id = Guid.NewGuid().ToString();
        await JS.InvokeVoid("FirestoreSubscribe", [id, docPath, "OnFirebaseReceived"]);
        Callbacks[id] = data => func.Invoke(id, data as T);
        return id;
    }

    public async Task Unsubscribe(string id) {
        await JS.InvokeVoid("FirestoreUnsubscribe", id);
        Callbacks.Remove(id);
    }

    [JSInvokable]
    public static async Task OnFirebaseReceived(string id, object data) {
        if (_instance.Callbacks.TryGetValue(id, out Action<object?>? func)) {
            func.Invoke(data);
        } else {
            await _instance.JS.InvokeVoid("console.error", $"Callback with id {id} not found");
        }
    }
}