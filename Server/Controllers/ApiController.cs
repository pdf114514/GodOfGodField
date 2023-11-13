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
    public async Task<GFSession> GetSession() {
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = "projects/godfield/databases/(default)";
        query["VER"] = "8";
        query["RID"] = "0"; // should be random?
        query["CVER"] = "22";
        query["X-HTTP-Session-Id"] = "gsessionid";
        query["$httpHeaders"] = $"X-Goog-Api-Client:gl-js/ fire/8.10.0\nContent-Type:text/plain\nAuthorization:Bearer {Request.Headers["Authorization"].First()!.Split(' ')[1]}\n";
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        var uri = uriBuilder.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uri) {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["count"] = "1",
                ["ofs"] = "0",
                ["req0__data__"] = JsonSerializer.Serialize(new {
                    database = "projects/godfield/databases/(default)",
                    addTarget = new {
                        documents = new {
                            documents = new object[] {
                                "projects/godfield/databases/(default)/documents/userCount/data"
                            }
                        },
                        targetId = 2
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
        var s = await response.Content.ReadAsStringAsync();
        Console.WriteLine(s);
        var json = JsonSerializer.Deserialize<JsonElement>(s[s.IndexOf("[[1,[{")..(s.IndexOf("}\n]]]") + "}\n]]]".Length)])![1][1][0].GetProperty("documentChange").GetProperty("document").GetProperty("fields");
        return new() {
            Training = json.GetProperty("training").GetProperty("integerValue").GetInt32(),
            Hidden = json.GetProperty("hidden").GetProperty("integerValue").GetInt32(),
            Duel = json.GetProperty("duel").GetProperty("integerValue").GetInt32()
        };
    }
}
