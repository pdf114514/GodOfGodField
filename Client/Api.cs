using System.Net.Http.Json;
using System.Text.Json;
using GodOfGodField.Shared;
// using Google.Apis.Auth.OAuth2;
// using Google.Cloud.Firestore;

namespace GodOfGodField.Client;

public class ApiClient {
    private readonly HttpClient Http;
    private readonly ApplicationState AppState;
    // private FirestoreDb? _Firestore;
    // private async Task<FirestoreDb> GetFirestore() => _Firestore ??= await new FirestoreDbBuilder() {
    //     ProjectId = "godfield",
    //     Credential = GoogleCredential.FromAccessToken(AppState.IdToken)
    // }.BuildAsync();

    public static readonly string FirebaseKey = "AIzaSyCBvMvZkHymK04BfEaERtbmELhyL8-mtAg";

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
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync())!.GetProperty("fields");
        return new() {
            Training = int.Parse(json.GetProperty("training").GetProperty("integerValue").GetString()!),
            Hidden = int.Parse(json.GetProperty("hidden").GetProperty("integerValue").GetString()!),
            Duel = int.Parse(json.GetProperty("duel").GetProperty("integerValue").GetString()!)
        };
    }

    public async Task<DuelRecord> GetDuelRecord() {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"https://firestore.googleapis.com/v1/projects/godfield/databases/(default)/documents/records/{AppState.LocalId}");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        using var response = await Http.SendAsync(request);
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
        return new() {
            Rating = int.Parse(json.GetProperty("rating").GetProperty("integerValue").GetString()!),
            GameCount = int.Parse(json.GetProperty("gameCount").GetProperty("integerValue").GetString()!),
            EnemyUserIds = json.GetProperty("enemyUserIds").GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => x.GetProperty("stringValue").GetString()!).ToArray()
        };
    }

    public async Task AddDuelUser(AddDuelUserRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/addDuelUser");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Lang = "ja", Mode = "duel", UserName = AppState.UserName }), null, "application/json");
        await Http.SendAsync(request);
    }

    public async Task<string> CreateRoom(CreateRoomRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/createRoom");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", Password = AppState.RoomPassword, UserName = AppState.UserName }), null, "application/json");
        using var response = await Http.SendAsync(request);
        return (await response.Content.ReadFromJsonAsync<CreateRoomResponse>())!.RoomId;
    }

    public async Task AddRoomUser(AddRoomUserRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/addRoomUser");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json");
        await Http.SendAsync(request);
    }

    public async Task RemoveRoomUser(RemoveRoomUserRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/removeRoomUser");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", RoomId = AppState.RoomId }), null, "application/json");
        await Http.SendAsync(request);
    }

    public Task SetEntryUser(int teamId) => SetEntryUser(new SetEntryUserRequest() { Mode = "hidden", RoomId = AppState.RoomId, Team = teamId });
    public async Task SetEntryUser(SetEntryUserRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/setEntryUser");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json");
        await Http.SendAsync(request);
    }

    public async Task ShuffleTeams(ShuffleTeamsRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/shuffleTeams");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", RoomId = AppState.RoomId }), null, "application/json");
        await Http.SendAsync(request);
    }

    public async Task AddGame(AddGameRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/addGame");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", RoomId = AppState.RoomId }), null, "application/json");
        await Http.SendAsync(request);
    }

    public async Task RemoveGame(RemoveGameRequest? data = null) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/removeGame");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data ?? new() { Mode = "hidden", RoomId = AppState.RoomId }), null, "application/json");
        await Http.SendAsync(request);
    }

    public Task UpdateGame(int[] itemIds, string? targetPlayerId = null) => UpdateGame(new UpdateGameRequest() { Mode = "hidden", RoomId = AppState.RoomId, Command = new() { ItemIds = itemIds, TargetPlayerId = targetPlayerId } });
    public async Task UpdateGame(UpdateGameRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://asia-northeast1-godfield.cloudfunctions.net/updateGame");
        request.Headers.Authorization = new("Bearer", AppState.IdToken);
        request.Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json");
        await Http.SendAsync(request);
    }
}