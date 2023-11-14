using Microsoft.AspNetCore.Mvc;
using GodOfGodField.Shared;
using System.Text.Json;
using System.Web;
using System.Net.Http.Headers;

namespace GodOfGodField.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ApiController : ControllerBase {
    public static readonly string FirebaseKey = "AIzaSyCBvMvZkHymK04BfEaERtbmELhyL8-mtAg";
    private readonly static HttpClient Http = new();
    private readonly static Random RNG = new();

    [HttpPost("signup")]
    public async Task<SignUpResponse> SignUp() {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/signupNewUser?key={FirebaseKey}") {
            Content = new StringContent(JsonSerializer.Serialize(new { returnSecureToken = true }), null, "application/json")
        };
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<SignUpResponse>(await response.Content.ReadAsStringAsync())!;
    }

    [HttpPost("getaccountinfo")]
    public async Task<AccountInfo> GetAccountInfo([FromBody] GetAccountInfoRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://www.googleapis.com/identitytoolkit/v3/relyingparty/getAccountInfo?key={FirebaseKey}") {
            Content = new StringContent(JsonSerializer.Serialize(new { idToken = data.IdToken }), null, "application/json")
        };
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<AccountInfo>(await response.Content.ReadAsStringAsync())!;
    }

    [HttpPost("refreshtoken")]
    public async Task<RefreshTokenResponse> RefreshToken([FromBody] RefreshTokenRequest data) {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"https://securetoken.googleapis.com/v1/token?key={FirebaseKey}") {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = data.RefreshToken
            })
        };
        using var response = await Http.SendAsync(request);
        return JsonSerializer.Deserialize<RefreshTokenResponse>(await response.Content.ReadAsStringAsync())!;
    }

    [HttpPost("getsession")]
    public async Task<GFSession> GetSession() => await GetSession(Request, 0, 2);

    public async Task<GFSession> GetSession(HttpRequest request, int requestId, int targetId) => await GetSession(request.Headers["Authorization"].First()!.Split(' ')[1], requestId, targetId);
    public async Task<GFSession> GetSession(string token, int requestId, int targetId) {
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = "projects/godfield/databases/(default)";
        query["VER"] = "8";
        query["RID"] = requestId.ToString();
        query["CVER"] = "22";
        query["X-HTTP-Session-Id"] = "gsessionid";
        query["$httpHeaders"] = $"X-Goog-Api-Client:gl-js/ fire/8.10.0\nContent-Type:text/plain\nAuthorization:Bearer {token}\n";
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        var uri = uriBuilder.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["count"] = "1",
                ["ofs"] = "0",
                ["req0___data__"] = JsonSerializer.Serialize(new {
                    database = "projects/godfield/databases/(default)",
                    addTarget = new {
                        documents = new {
                            documents = new object[] {
                                "projects/godfield/databases/(default)/documents/userCount/data"
                            }
                        },
                        targetId = targetId
                    }
                })
            })
        };
        using var response = await Http.SendAsync(request);
        var sessionId = JsonSerializer.Deserialize<List<JsonElement>>((await response.Content.ReadAsStringAsync()).Split('\n')[1])![0][1][1].GetString()!;
        var gSessionId = response.Headers.GetValues("X-Http-Session-Id").First()!;
        return new() {
            SessionId = sessionId,
            GSessionId = gSessionId
        };
    }

    [HttpPost("getusercount")]
    public async Task<UserCount> GetUserCount() {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://firestore.googleapis.com/v1/projects/godfield/databases/(default)/documents/userCount/data");
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(Request.Headers.Authorization.First()!);
        using var response = await Http.SendAsync(request);
        var json = JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync())!.GetProperty("fields");
        return new() {
            Training = int.Parse(json.GetProperty("training").GetProperty("integerValue").GetString()!),
            Hidden = int.Parse(json.GetProperty("hidden").GetProperty("integerValue").GetString()!),
            Duel = int.Parse(json.GetProperty("duel").GetProperty("integerValue").GetString()!)
        };
    }

    // Revoke the session?
    public async Task Unknown1(GFSession session, int requestId, int targetId) {
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = "projects/godfield/databases/(default)";
        query["gsessionid"] = session.GSessionId;
        query["SID"] = session.SessionId;
        query["RID"] = requestId.ToString();
        query["AID"] = "4";
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()) {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["count"] = "1",
                ["ofs"] = "1",
                ["req0___data__"] = JsonSerializer.Serialize(new {
                    database = "projects/godfield/databases/(default)",
                    removeTarget = targetId
                })
            })
        };
        using var response = await Http.SendAsync(request);
    }
}
