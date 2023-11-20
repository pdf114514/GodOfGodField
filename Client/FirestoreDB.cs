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
    private ListeningChannel Channel = new();

    private readonly HttpClient Http;
    private readonly ApiClient Api;
    private readonly ApplicationState AppState;

    public FirestoreDB(HttpClient httpClient, ApiClient api, ApplicationState appState) {
        Http = httpClient;
        Api = api;
        AppState = appState;

        async void onCloseCallback () {
            Console.WriteLine("Channel closed, Reconecting...");
            await ListenChannel();
        };
        Channel.OnClose += onCloseCallback;
    }

    public async Task<ListeningChannel> GetChannel() {
        if (Channel.IsOpen) return Channel;
        await ListenChannel();
        return Channel;
    }

    private const string ZXCharacters = "abcdefghijklmnopqrstuvwxyz0123456789";
    private string GenerateZX() {
        var sb = new StringBuilder();
        for (var i = 0; i < 12; i++) sb.Append(ZXCharacters[RNG.Next(0, ZXCharacters.Length)]);
        return sb.ToString();
    }

    private async Task ListenChannel() {
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
            await ResumeTargets(Channel.ResumeToken!);
            await ListenChannel();
        }
        Console.WriteLine($"New channel connected!");
        Channel.ReadResponse(response);
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
        private Task? ReadingTask = null;
        private readonly Dictionary<string, Action<JsonDocument>> DocumentChangeListeners = new();
        private readonly Dictionary<string, Action> DocumentDeleteListeners = new();

        public bool IsOpen { get; private set; } = false;
        public string? ResumeToken { get; private set; }

        public event Action? OnClose;
        public event Action<JsonDocument> OnObjectReceived;

        public ListeningChannel() {
            OnObjectReceived += ObjectReceived;
        }

        public void ReadResponse(HttpResponseMessage response) {
            if (ReadingTask is not null && !ReadingTask.IsCompleted) ReadingTask.Wait();
            ReadingTask = Read(response);
        }

        private async Task Read(HttpResponseMessage response) {
            IsOpen = true;
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            var c = 0;
            var sb = new StringBuilder();
            while (await reader.ReadLineAsync() is var line && line is not null) {
                var s = line.Count(x => x == '[');
                var l = line.Count(x => x == ']');
                c += s - l;
                sb.Append(line);
                if (l != 0 && c == 0) {
                    var r = sb.ToString();
                    sb.Clear();
                    var x = r[r.IndexOf('[')..(r.LastIndexOf(']') + 1)];
                    Console.WriteLine($"Length: {x.Length}");
                    var obj = JsonSerializer.Deserialize<JsonDocument>(x)!;
                    Console.WriteLine(JsonSerializer.Serialize(obj, new JsonSerializerOptions() { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
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
                foreach (var o2 in o[1].EnumerateArray()) {
                    if (o2.GetRawText() == "\"noop\"") continue;

                    if (o2.TryGetProperty("targetChange", out var tc) && tc.TryGetProperty("resumeToken", out var resumeToken)) {
                        ResumeToken = resumeToken.GetString()!;
                        Console.WriteLine($"ResumeToken: {ResumeToken}");
                    } else if (o2.TryGetProperty("documentChange", out var dc)) {
                        var document = dc.GetProperty("document");
                        var documentPath = document.GetProperty("name").GetString()!;
                        Console.WriteLine($"DocumentChange: {documentPath}");
                        if (DocumentChangeListeners.ContainsKey(documentPath)) {
                            DocumentChangeListeners[documentPath](document.GetProperty("fields").Deserialize<JsonDocument>()!);
                        }
                    } else if (o2.TryGetProperty("documentDelete", out var dd)) {
                        var documentPath = dd.GetProperty("document").GetString()!;
                        Console.WriteLine($"DocumentDelete: {documentPath}");
                        if (DocumentDeleteListeners.ContainsKey(documentPath)) {
                            DocumentDeleteListeners[documentPath]();
                        }
                    }
                }
            }
        }

        public void AddDocumentChangeListener(string documentPath, Action<JsonDocument> document) => DocumentChangeListeners.Add(documentPath, document);
        public void RemoveDocumentChangeListener(string documentPath) => DocumentChangeListeners.Remove(documentPath);
        public void RemoveAllDocumentChangeListeners() => DocumentChangeListeners.Clear();

        public void AddDocumentDeleteListener(string documentPath, Action document) => DocumentDeleteListeners.Add(documentPath, document);
        public void RemoveDocumentDeleteListener(string documentPath) => DocumentDeleteListeners.Remove(documentPath);
        public void RemoveAllDocumentDeleteListeners() => DocumentDeleteListeners.Clear();
    }
}