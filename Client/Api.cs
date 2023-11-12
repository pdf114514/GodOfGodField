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

    public async Task<GFSession> GetSession() {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/getsession");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        return (await (await Http.SendAsync(request)).Content.ReadFromJsonAsync<GFSession>())!;
    }

    public async Task<UserCount> GetUserCount() => (await (await Http.PostAsJsonAsync<GFSession>("/api/getusercount", new() { GSessionId = AppState.GSessionId, SessionId = AppState.SessionId })).Content.ReadFromJsonAsync<UserCount>())!;
}