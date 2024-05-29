using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class Firebase {
    static Firebase _instance = default!;
    readonly IJSInProcessRuntime JS;
    readonly Dictionary<string, Action<object?>> Callbacks = [];

    public Firebase(IJSInProcessRuntime js) {
        _instance = this;
        JS = js;
    }

    public ValueTask SignIn() => JS.InvokeVoidAsync("FirebaseSignIn");

    public async Task<string> Subscribe(string docPath, Action<string, object?> func) {
        var id = Guid.NewGuid().ToString();
        await JS.InvokeVoid("FirestoreSubscribe", [id, docPath, "OnFirebaseReceived"]);
        Callbacks[id] = data => func.Invoke(id, data);
        return id;
    }

    public async Task Unsubscribe(string id) {
        await JS.InvokeVoid("FirestoreUnsubscribe", id);
        Callbacks.Remove(id);
    }

    public async Task UnsubscribeAll() {
        foreach (var id in Callbacks.Keys) {
            await JS.InvokeVoid("FirestoreUnsubscribe", id);
        }
        Callbacks.Clear();
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