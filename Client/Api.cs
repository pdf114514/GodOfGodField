using System.Net.Http.Json;
using System.Text.Json;
using GodOfGodField.Shared;

namespace GodOfGodField.Client;

public class ApiClient {
    private readonly HttpClient Http;
    private readonly ApplicationState AppState;

    public static readonly string FirebaseKey = "AIzaSyCBvMvZkHymK04BfEaERtbmELhyL8-mtAg";

    public bool AutoRefresh { get; init; } = true;
    private DateTime LastRefresh = DateTime.MinValue;

    public ApiClient(HttpClient http, ApplicationState appState) {
        Http = http;
        AppState = appState;
    }

    public async Task<SignUpResponse> SignUp() {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={FirebaseKey}") {
            Content = new StringContent(JsonSerializer.Serialize(new { returnSecureToken = true }), null, "application/json")
        };
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<SignUpResponse>(await response.Content.ReadAsStringAsync())!;
    }

    public async Task<AccountInfo> GetAccountInfo() {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key={FirebaseKey}") {
            Content = new StringContent(JsonSerializer.Serialize(new { idToken = AppState.IdToken }), null, "application/json")
        };
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<AccountInfo>(await response.Content.ReadAsStringAsync())!;
    }

    public async Task<RefreshTokenResponse> RefreshToken() {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://securetoken.googleapis.com/v1/token?key={FirebaseKey}") {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = AppState.RefreshToken
            })
        };
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<RefreshTokenResponse>(await response.Content.ReadAsStringAsync())!;
    }

    public async Task Refresh() {
        if (DateTime.Now - LastRefresh < TimeSpan.FromMinutes(5)) throw new("Cannot refresh token more than once in 5 minutes.");
        var refresh = await RefreshToken();
        AppState.IdToken = refresh.IdToken;
        AppState.RefreshToken = refresh.RefreshToken;
        AppState.ExpiresIn = int.Parse(refresh.ExpiresIn);
        AppState.LocalId = refresh.UserId;
        LastRefresh = DateTime.Now;
    }

    public async Task<GFSession> GetSession() {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/getsession");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        var response = await Http.SendAsync(request);
        if (AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            return await GetSession();
        }
        return (await response.Content.ReadFromJsonAsync<GFSession>())!;
    }

public async Task<UserCount> GetUserCount() {akimotii
        // This is not working
        // var x = await (await GetFirestore()).Collection("userCount").Document("data").GetSnapshotAsync();
        // return new() {
        //     Training = int.Parse(x.GetValue<int>("training").ToString()),
        //     Hidden = int.Parse(x.GetValue<int>("hidden").ToString()),
        //     Duel = int.Parse(x.GetValue<int>("duel").ToString())
        // };
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://firestore.googleapis.com/v1/projects/godfield/databases/(default)/documents/userCount/data");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        using var response = await Http.SendAsync(request);
        if (AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            return await GetUserCount();
        }
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync())!.GetProperty("fields");
        return new() {
            Training = json.GetProperty("training").GetIntValue(),
            Hidden = json.GetProperty("hidden").GetIntValue(),
            Duel = json.GetProperty("duel").GetIntValue()
        };
    }

    public async Task<DuelRecord> GetDuelRecord() {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://firestore.googleapis.com/v1/projects/godfield/databases/(default)/documents/records/{AppState.LocalId}");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        using var response = await Http.SendAsync(request);
        if (AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            return await GetDuelRecord();
        }
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        return new() {
            Rating = json.GetProperty("fields").GetProperty("rating").GetIntValue(),
            GameCount = json.GetProperty("fields").GetProperty("gameCount").GetIntValue(),
            EnemyUserIds = json.GetProperty("enemyUserIds").GetArrayEnumerator().Select(x => x.GetStringValue()).ToArray()
        };
    }

    public async Task AddDuelUser(AddDuelUserRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/addDuelUser");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Lang = "ja", Mode = "duel", UserName = AppState.UserName }), null, "application/json");
        if (await Http.SendAsync(request) is var response && AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            await AddDuelUser(data);
        }
    }

    public async Task<string> CreateRoom(CreateRoomRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/createRoom");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", Password = AppState.RoomPassword, UserName = AppState.UserName }), null, "application/json");
        using var response = await Http.SendAsync(request);
        if (AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            return await CreateRoom(data);
        }
        return (await response.Content.ReadFromJsonAsync<CreateRoomResponse>())!.RoomId;
    }

    public async Task AddRoomUser(AddRoomUserRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/addRoomUser");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json");
        var response = await Http.SendAsync(request);
        if (AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            await AddRoomUser(data);
        }
    }

    public async Task RemoveRoomUser(RemoveRoomUserRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/removeRoomUser");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", RoomId = AppState.RoomId }), null, "application/json");
        if (await Http.SendAsync(request) is var response && AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            await RemoveRoomUser(data);
        }
    }

    public Task SetEntryUser(int teamId) => SetEntryUser(new SetEntryUserRequest() { Mode = "hidden", RoomId = AppState.RoomId, Team = teamId });
    public async Task SetEntryUser(SetEntryUserRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/setEntryUser");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json");
        if (await Http.SendAsync(request) is var response && AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            await SetEntryUser(data);
        }
    }

    public async Task ShuffleTeams(ShuffleTeamsRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/shuffleTeams");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", RoomId = AppState.RoomId }), null, "application/json");
        if (await Http.SendAsync(request) is var response && AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            await ShuffleTeams(data);
        }
    }

    public async Task AddGame(AddGameRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/addGame");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", RoomId = AppState.RoomId }), null, "application/json");
        if (await Http.SendAsync(request) is var response && AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            await AddGame(data);
        }
    }

    public async Task RemoveGame(RemoveGameRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/removeGame");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", RoomId = AppState.RoomId }), null, "application/json");
        if (await Http.SendAsync(request) is var response && AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            await RemoveGame(data);
        }
    }

    public Task UpdateGame(int[] itemIds, int? targetPlayerId = null) => UpdateGame(new UpdateGameRequest() { Mode = "hidden", RoomId = AppState.RoomId, Command = new() { ItemIds = itemIds, TargetPlayerId = targetPlayerId } });
    public async Task UpdateGame(UpdateGameRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/updateGame");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json");
        if (await Http.SendAsync(request) is var response && AutoRefresh && !response.IsSuccessStatusCode) {
            await Refresh();
            await UpdateGame(data);
        }
    }
}