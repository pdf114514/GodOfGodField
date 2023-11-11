using Microsoft.AspNetCore.Mvc;
using GodOfGodField.Shared;
using System.Text.Json;
using System.Web;

namespace GodOfGodField.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase {
    public static readonly string FirebaseKey = "AIzaSyCBvMvZkHymK04BfEaERtbmELhyL8-mtAg";
    private readonly static HttpClient Http = new();

    [HttpPost]
    public async Task<SignUpResponse> SignUp() {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={FirebaseKey}") {
            Content = new StringContent(JsonSerializer.Serialize(new { returnSecureToken = true }), null, "application/json")
        };
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<SignUpResponse>(await response.Content.ReadAsStringAsync())!;
    }

    [HttpPost]
    public async Task<AccountInfo> GetAccountInfo([FromBody] GetAccountInfoRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key={FirebaseKey}") {
            Content = new StringContent(JsonSerializer.Serialize(new { idToken = data.IdToken }), null, "application/json")
        };
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<AccountInfo>(await response.Content.ReadAsStringAsync())!;
    }

    [HttpPost]
    public async Task<GFSession> GetSession([FromBody] GetGFSessionRequest data) {
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = "projects/godfield/databases/(default)";
        query["VER"] = "8";
        // query["RID"] = int REQUEST_ID;
        query["CVER"] = "22";
        query["X-HTTP-Session-Id"] = "gsessionid";
        query["$httpHeaders"] = $"X-Goog-Api-Client:gl-js/ fire/8.10.0\nContent-Type:text/plain\nAuthorization:Bearer {Request.Headers["Authorization"].First()!.Split(' ')[1]}\n";
        uriBuilder.Query = query.ToString();
        var uri = uriBuilder.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) {
            Content = new StringContent(JsonSerializer.Serialize(data), null, "application/json")
        };
        using var response = await Http.SendAsync(request);
        var sessionId = JsonSerializer.Deserialize<List<JsonElement>>((await response.Content.ReadAsStringAsync()).Split('\n')[1])![0][1][1].GetString()!;
        var gSessionId = response.Headers.GetValues("X-Http-Session-Id").First()!;
        return new() {
            SessionId = sessionId,
            GSessionId = gSessionId
        };
    }

    [HttpPost]
    public async Task<UserCount> GetUserCount([FromBody] GFSession session) {
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = "projects/godfield/databases/(default)";
        query["gsessionid"] = session.GSessionId;
        query["VER"] = "8";
        query["RID"] = "rpc";
        query["SID"] = session.SessionId;
        query["CI"] = "0";
        query["AID"] = "0";
        query["TYPE"] = "xmlhttp";
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        var uri = uriBuilder.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await Http.SendAsync(request);
        var json = JsonSerializer.Deserialize<JsonElement>((await response.Content.ReadAsStringAsync()).Split('\n')[1])![1][1][0].GetProperty("documentChange").GetProperty("document").GetProperty("fields");
        return new() {
            Training = json.GetProperty("training").GetProperty("integerValue").GetInt32(),
            Hidden = json.GetProperty("hidden").GetProperty("integerValue").GetInt32(),
            Duel = json.GetProperty("duel").GetProperty("integerValue").GetInt32()
        };
    }
}
