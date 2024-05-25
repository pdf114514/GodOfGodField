using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public static class IJSRuntimeExtension {
    public static ValueTask InvokeVoid(this IJSRuntime jsRuntime, string identifier, params object?[]? args) => jsRuntime.InvokeVoidAsync(identifier, args);
    public static async Task CLog(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("console.log", args);
    public static async Task CError(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("console.error", args);
    public static async Task Alert(this IJSRuntime jsRuntime, params object?[]? args) => await jsRuntime.InvokeVoidAsync("alert", args);
    public static async Task LSSetItem(this IJSRuntime jsRuntime, string key, string value) => await jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
    public static async ValueTask<string?> LSGetItem(this IJSRuntime jsRuntime, string key) => await jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    public static async Task LSRemoveItem(this IJSRuntime jsRuntime, string key) => await jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    public static async Task<IJSObjectReference> GetElementById(this IJSRuntime jsRuntime, string id) => await jsRuntime.InvokeAsync<IJSObjectReference>("document.getElementById", id);
    public static async Task<IJSObjectReference> GetElementByClassName(this IJSRuntime jsRuntime, string className) => await (await jsRuntime.InvokeAsync<IJSObjectReference>("document.getElementsByClassName", className)).InvokeAsync<IJSObjectReference>("at", 0);
    public static async Task SetImage(this IJSRuntime jsRuntime, IJSObjectReference element, DotNetStreamReference stream) => await jsRuntime.InvokeVoidAsync("SetImage", element, stream);
    public static async Task RemoveImage(this IJSRuntime jsRuntime, IJSObjectReference element) => await jsRuntime.InvokeVoidAsync("RemoveImage", element);
    public static async Task SetBackgroundImage(this IJSRuntime jsRuntime, IJSObjectReference element, DotNetStreamReference stream) => await jsRuntime.InvokeVoidAsync("SetBackgroundImage", element, stream);
    public static async Task PlayAudio(this IJSRuntime jsRuntime, DotNetStreamReference stream) => await jsRuntime.InvokeVoidAsync("PlayAudio", stream);
    public static ValueTask ShowMessage(this IJSRuntime jsRuntime, string message) => jsRuntime.InvokeVoid("ShowMessage", message);
    public static ValueTask<string> CreateObjectURL(this IJSRuntime jsRuntime, DotNetStreamReference stream) => jsRuntime.InvokeAsync<string>("CreateObjectURL", stream);
    public static ValueTask<IJSInProcessObjectReference> CreateImageFromURL(this IJSRuntime jsRuntime, string url) => jsRuntime.InvokeAsync<IJSInProcessObjectReference>("CreateImageFromURL", url);
    public static ValueTask RevokeObjectURL(this IJSRuntime jsRuntime, string url) => jsRuntime.InvokeVoidAsync("RevokeObjectURL", url);
}

public static class IJSInProcessRuntimeExtension {
    public static IJSInProcessObjectReference GetElementById(this IJSInProcessRuntime jsRuntime, string id) => jsRuntime.Invoke<IJSInProcessObjectReference>("document.getElementById", id);
    public static IJSInProcessObjectReference GetElementByClassName(this IJSInProcessRuntime jsRuntime, string className) => jsRuntime.Invoke<IJSInProcessObjectReference>("document.getElementsByClassName", className).Invoke<IJSInProcessObjectReference>("at", 0);
    public static string? LSGetItem(this IJSInProcessRuntime jsRuntime, string key) => jsRuntime.Invoke<string>("localStorage.getItem", key);
    public static void LSSetItem(this IJSInProcessRuntime jsRuntime, string key, string value) => jsRuntime.InvokeVoid("localStorage.setItem", key, value);
    public static void LSRemoveItem(this IJSInProcessRuntime jsRuntime, string key) => jsRuntime.InvokeVoid("localStorage.removeItem", key);
}

public static class IJSObjectReferenceExtension {
    public static ValueTask<T> GetProperty<T>(this IJSObjectReference jsObjRef, string identifier) => jsObjRef.InvokeAsync<T>("getProperty", identifier);
    public static ValueTask SetProperty<T>(this IJSObjectReference jsObjRef, string identifier, T value) => jsObjRef.InvokeVoidAsync("setProperty", identifier, value);
}

public static class IJSInProcessObjectReferenceExtension {
    public static T GetProperty<T>(this IJSInProcessObjectReference jsObjRef, string identifier) => jsObjRef.Invoke<T>("getProperty", identifier);
    public static void SetProperty<T>(this IJSInProcessObjectReference jsObjRef, string identifier, T value) => jsObjRef.InvokeVoid("setProperty", identifier, value);
}