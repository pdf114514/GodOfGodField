using System.Text;
using System.Text.Json;
using System.Web;
using GodOfGodField.Client;
using GodOfGodField.Shared;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

public class FirestoreDB {
    public const string ProjectId = "godfield";
    public const string FirestoreKey = "AIzaSyCBvMvZkHymK04BfEaERtbmELhyL8-mtAg";
    public const string ListenChannelUrl = "https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel";
    private const string InvalidSessionMessage = "Your client has issued a malformed or illegal request. Unknown SID";

    private readonly Random RNG = new();
    private int AndroidId = 0;
    private int TargetId = 0;
    private int Offset = -1;
    private readonly Dictionary<int, string> Targets = new();
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
        var uriBuilder = new UriBuilder(ListenChannelUrl);
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
        using var request = new HttpRequestMessage(HttpMethod.Get, uriBuilder.ToString());
        request.SetBrowserResponseStreamingEnabled(true); // ,,
        var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && (await response.Content.ReadAsStringAsync()).Contains(InvalidSessionMessage)) {
            Console.WriteLine("Session might be expired");
            await Api.Refresh();
            await ResumeTargets(Channel.ResumeToken!);
            return await ListenChannel();
        }
        var channel = new ListeningChannel(response);
        Console.WriteLine($"New channel connected!");
        async void onCloseCallback () {
            Console.WriteLine("Channel closed, Reconecting...");
            Channel = null!;
            var targets = Targets;
            foreach(var target in Targets) await RemoveTarget(target.Key);
            foreach(var target in targets) await AddTarget(target.Value);
            Channel = await ListenChannel();
            channel.OnClose -= onCloseCallback;
        };
        channel.OnClose += onCloseCallback;
        return channel;
    }

    private async Task<int> AddTargetWithSession(string documentPath) {
        var requestId = RNG.Next(10000, 60001);
        var targetId = TargetId += 2;
        var uriBuilder = new UriBuilder(ListenChannelUrl);
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
                ["ofs"] = (++Offset).ToString(),
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
        Targets.Add(targetId, documentPath);
        await GetChannel();
        return targetId;
    }

    public async Task<int> AddTarget(string documentPath) {
        if (Session is null) return await AddTargetWithSession(documentPath);
        var targetId = TargetId += 2;
        Console.WriteLine($"AddTarget {targetId}, {documentPath}");
        var uriBuilder = new UriBuilder(ListenChannelUrl);
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
                ["ofs"] = (++Offset).ToString(),
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
        Targets.Add(targetId, documentPath);
        return targetId;
    }

    public async Task ResumeTargets(string resumeToken) {
        var uriBuilder = new UriBuilder(ListenChannelUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = $"projects/{ProjectId}/databases/(default)";
        query["VER"] = "8";
        query["RID"] = RNG.Next(10000, 60001).ToString();
        query["CVER"] = "22";
        query["X-HTTP-Session-Id"] = "gsessionid";
        query["$httpHeaders"] = $"X-Goog-Api-Client:gl-js/ fire/8.10.0\nContent-Type:text/plain\nAuthorization:Bearer {AppState.IdToken}\n";
        query["zx"] = GenerateZX();
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        Offset = -1;
        var data = new Dictionary<string, string>() {
            ["count"] = Targets.Count.ToString(),
            ["ofs"] = (++Offset).ToString()
        };
        for (var i = 0; i < Targets.Count; i++) {
            data[$"req{i}___data__"] = JsonSerializer.Serialize(new {
                database = $"projects/{ProjectId}/databases/(default)",
                addTarget = new {
                    documents = new {
                        documents = new[] { Targets.Values.ElementAt(i) }
                    },
                    targetId = Targets.Keys.ElementAt(i),
                    resumeToken = resumeToken
                }
            });
        }
        var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()) {
            Content = new FormUrlEncodedContent(data)
        };
        var response = await Http.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && (await response.Content.ReadAsStringAsync()).Contains("Your client has issued a malformed or illegal request. Unknown SID")) {
            await Api.Refresh();
            await ResumeTargets(resumeToken);
        }
        var sessionId = JsonSerializer.Deserialize<List<JsonElement>>((await response.Content.ReadAsStringAsync()).Split('\n')[1])![0][1][1].GetString()!;
        var gSessionId = response.Headers.GetValues("X-Http-Session-Id").First()!;
        Session = new() { SessionId = sessionId, GSessionId = gSessionId };
    }

    public async Task RemoveTarget(int targetId) {
        Console.WriteLine($"RemoveTarget {targetId}, {Targets[targetId]}");
        var uriBuilder = new UriBuilder(ListenChannelUrl);
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
        Targets.Remove(targetId);
    }

    public async Task RemoveAllTargets() {
        foreach(var target in Targets) await RemoveTarget(target.Key);
    }

    public class ListeningChannel {
        private HttpResponseMessage Response;

        public bool IsOpen { get; private set; } = true;
        public string? ResumeToken { get; private set; }

        public event Action? OnClose;
        public event Action<JsonDocument> OnObjectReceived;

        public ListeningChannel(HttpResponseMessage response) {
            Response = response;
            OnObjectReceived += ObjectReceived;
        }

        public async Task Test() {
            using var stream = await Response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            var c = 0;
            var r = "";
            while (await reader.ReadLineAsync() is var line && line is not null) {
                var s = line.Count(x => x == '[');
                var l = line.Count(x => x == ']');
                c += s - l;
                r += line;
                if (l != 0 && c == 0) {
                    var x = r[r.IndexOf('[')..(r.LastIndexOf(']') + 1)];
                    Console.WriteLine($"Length: {x.Length}");
                    var obj = JsonSerializer.Deserialize<JsonDocument>(x)!;
                    Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
                    r = "";
                    OnObjectReceived?.Invoke(obj);
                } else if (c == 0) {
                    Console.WriteLine(line);
                }
            }
            IsOpen = false;
            OnClose?.Invoke();
        }

        private void ObjectReceived(JsonDocument obj) {
            var l = obj.Deserialize<List<List<JsonElement>>>()!;
            foreach (var o in l) {
                foreach (var o2 in o[1].EnumerateArray().Where(x => x.GetRawText() != "\"noop\"")) {
                    if (o2.TryGetProperty("targetChange", out var p) && p.TryGetProperty("resumeToken", out var resumeToken)) {
                        ResumeToken = resumeToken.GetString()!;
                        Console.WriteLine($"ResumeToken: {ResumeToken}");
                    }
                }
            }
        }
    }
}