using System.Text;
using System.Text.Json;
using System.Web;
using GodOfGodField.Client;
using GodOfGodField.Shared;

public class FirestoreDB {
    public const string ProjectId = "godfield";
    public const string FirestoreKey = "AIzaSyCBvMvZkHymK04BfEaERtbmELhyL8-mtAg";
    private const string InvalidSessionMessage = "Your client has issued a malformed or illegal request. Unknown SID";

    private readonly Random RNG = new();
    private int AndroidId = 0;
    private int TargetId = 0;
    private int Offset = -1;
    private GFSession Session = null!;
    private ListeningChannel Channel = null!;

    private readonly HttpClient Http;
    private readonly ApiClient Api;
    private readonly ApplicationState AppState;

    public FirestoreDB(HttpClient httpClient, ApiClient api, ApplicationState appState) {
        Http = httpClient;
        Api = api;
        AppState = appState;
    }

    public async Task<ListeningChannel> GetChannel() => Channel is not null && Channel.IsOpen ? Channel : Channel = await ListenChannel();

    private const string ZXCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
    private string GenerateZX() {
        var sb = new StringBuilder();
        for (var i = 0; i < 12; i++) sb.Append(ZXCharacters[RNG.Next(0, ZXCharacters.Length)]);
        return sb.ToString();
    }

    private async Task<ListeningChannel> ListenChannel() {
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = $"projects/{ProjectId}/databases/(default)";
        query["gsessionid"] = Session.GSessionId;
        query["VER"] = "8";
        query["RID"] = "rpc";
        query["SID"] = Session.SessionId;
        query["CI"] = "0"; // case insensitive
        query["AID"] = AndroidId++.ToString();
        query["TYPE"] = "xmlhttp";
        query["zx"] = GenerateZX();
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        var response = await Http.SendAsync(new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString()));
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && (await response.Content.ReadAsStringAsync()).Contains(InvalidSessionMessage)) {
            await Api.Refresh();
            return await ListenChannel();
        }
        var channel = new ListeningChannel(response);
        Console.WriteLine($"New channel connected!");
        channel.OnClose += async () => {
            Console.WriteLine("Channel closed, Reconecting...");
            Channel = await ListenChannel();
        };
        return channel;
    }

    private async Task<int> AddTargetWithSession(string documentPath) {
        var requestId = RNG.Next(10000, 60001);
        var targetId = TargetId += 2;
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = $"projects/{ProjectId}/databases/(default)";
        query["VER"] = "8";
        query["RID"] = requestId.ToString();
        query["CVER"] = "22";
        query["X-HTTP-Session-Id"] = "gsessionid";
        query["$httpHeaders"] = $"X-Goog-Api-Client:gl-js/ fire/8.10.0\nContent-Type:text/plain\nAuthorization:Bearer {AppState.IdToken}\n";
        query["zx"] = GenerateZX();
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()) {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                ["count"] = "1",
                ["ofs"] = Offset++.ToString(),
                ["req0___data__"] = JsonSerializer.Serialize(new {
                    database = $"projects/{ProjectId}/databases/(default)",
                    addTarget = new {
                        documents = new {
                            documents = new[] { documentPath }
                        },
                        targetId = targetId
                    }
                
                })
            })
        };
        var response = await Http.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && (await response.Content.ReadAsStringAsync()).Contains("Your client has issued a malformed or illegal request. Unknown SID")) {
            await Api.Refresh();
            return await AddTargetWithSession(documentPath);
        }
        var sessionId = JsonSerializer.Deserialize<List<JsonElement>>((await response.Content.ReadAsStringAsync()).Split('\n')[1])![0][1][1].GetString()!;
        var gSessionId = response.Headers.GetValues("X-Http-Session-Id").First()!;
        Session = new() { SessionId = sessionId, GSessionId = gSessionId };
        await GetChannel();
        return targetId;
    }

    public async Task<int> AddTarget(string documentPath) {
        if (Session is null) return await AddTargetWithSession(documentPath);
        var targetId = TargetId += 2;
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = $"projects/{ProjectId}/databases/(default)";
        query["VER"] = "8";
        query["gsessionid"] = Session.GSessionId;
        query["SID"] = Session.SessionId;
        query["RID"] = RNG.Next(10000, 60001).ToString();
        query["AID"] = AndroidId++.ToString();
        query["zx"] = GenerateZX();
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()) {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                ["count"] = "1",
                ["ofs"] = Offset++.ToString(),
                ["req0___data__"] = JsonSerializer.Serialize(new {
                    database = $"projects/{ProjectId}/databases/(default)",
                    addTarget = new {
                        documents = new {
                            documents = new[] { documentPath }
                        },
                        targetId = targetId
                    }
                })
            })
        };
        var response = await Http.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && (await response.Content.ReadAsStringAsync()).Contains(InvalidSessionMessage)) {
            await Api.Refresh();
            return await AddTarget(documentPath);
        }
        return targetId;
    }

    public async Task RemoveTarget(int targetId) {
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = $"projects/{ProjectId}/databases/(default)";
        query["VER"] = "8";
        query["gsessionid"] = Session.GSessionId;
        query["SID"] = Session.SessionId;
        query["RID"] = RNG.Next(10000, 60001).ToString();
        query["AID"] = AndroidId++.ToString();
        query["zx"] = GenerateZX();
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()) {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                ["count"] = "1",
                ["ofs"] = Offset.ToString(),
                ["req0___data__"] = JsonSerializer.Serialize(new {
                    database = $"projects/{ProjectId}/databases/(default)",
                    removeTarget = new {
                        targetId = targetId
                    }
                })
            })
        };
        var response = await Http.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && (await response.Content.ReadAsStringAsync()).Contains(InvalidSessionMessage)) {
            await Api.Refresh();
            await RemoveTarget(targetId);
        }
    }

    public class ListeningChannel {
        private HttpResponseMessage Response;

        public bool IsOpen { get; private set; } = true;

        public event Action? OnClose;

        public ListeningChannel(HttpResponseMessage response) {
            Response = response;
        }

        public async Task Test() {
            var stream = await Response.Content.ReadAsStreamAsync();
            var reader = new StreamReader(stream);
            while (!reader.EndOfStream) {
                var line = await reader.ReadLineAsync();
                Console.WriteLine($"NEW LINE: {line}");
            }
            OnClose?.Invoke();
        }
    }
}