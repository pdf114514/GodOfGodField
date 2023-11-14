using System.Net.Http.Json;
using System.Text.Json;
using GodOfGodField.Shared;
using Microsoft.JSInterop;

namespace GodOfGodField.Client;

public class ApiClient {
    private readonly HttpClient Http;
    private readonly ApplicationState AppState;

    public ApiClient(HttpClient http, ApplicationState appState) {
        Http = http;
        AppState = appState;
    }

    public async Task<SignUpResponse> SignUp() => (await (await Http.PostAsync("/api/signup", null)).Content.ReadFromJsonAsync<SignUpResponse>())!;

    public async Task<AccountInfo> GetAccountInfo() => (await (await Http.PostAsJsonAsync<GetAccountInfoRequest>("/api/getaccountinfo", new() { IdToken = AppState.IdToken })).Content.ReadFromJsonAsync<AccountInfo>())!;

    public async Task<RefreshTokenResponse> RefreshToken() => (await (await Http.PostAsJsonAsync<RefreshTokenRequest>("/api/refreshtoken", new() { RefreshToken = AppState.RefreshToken })).Content.ReadFromJsonAsync<RefreshTokenResponse>())!;

    public async Task Refresh() {
        var refresh = await RefreshToken();
        AppState.IdToken = refresh.IdToken;
        AppState.RefreshToken = refresh.RefreshToken;
        AppState.ExpiresIn = int.Parse(refresh.ExpiresIn);
        AppState.LocalId = refresh.UserId;
        await AppState.Save();
    }

    public async Task<GFSession> GetSession() {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/getsession");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        return (await (await Http.SendAsync(request)).Content.ReadFromJsonAsync<GFSession>())!;
    }

    public async Task<UserCount> GetUserCount() {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://firestore.googleapis.com/v1/projects/godfield/databases/(default)/documents/userCount/data");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        using var response = await Http.SendAsync(request);
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync())!.GetProperty("fields");
        return new() {
            Training = int.Parse(json.GetProperty("training").GetProperty("integerValue").GetString()!),
            Hidden = int.Parse(json.GetProperty("hidden").GetProperty("integerValue").GetString()!),
            Duel = int.Parse(json.GetProperty("duel").GetProperty("integerValue").GetString()!)
        };
    }
}