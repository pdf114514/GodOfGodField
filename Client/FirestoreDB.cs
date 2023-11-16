using System.Text.Json;
using System.Web;
using GodOfGodField.Client;

public class FirestoreDB {
    public const string ProjectId = "godfield";
    public const string FirestoreKey = "AIzaSyCBvMvZkHymK04BfEaERtbmELhyL8-mtAg";

    private readonly Random RNG = new();

    private readonly HttpClient Http;
    private readonly ApiClient Api;
    private readonly ApplicationState AppState;

    public FirestoreDB(HttpClient httpClient, ApiClient api, ApplicationState appState) {
        Http = httpClient;
        Api = api;
        AppState = appState;
    }

    public async Task<ListeningChannel> ListenChannel(string documentPath) {
        var requestId = RNG.Next(1, 51) * 2;
        var targetId = RNG.Next(1, 51) * 2;
        var uriBuilder = new UriBuilder("https://firestore.googleapis.com/google.firestore.v1.Firestore/Listen/channel");
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
        query["database"] = "projects/godfield/databases/(default)";
        query["VER"] = "8";
        query["RID"] = requestId.ToString();
        query["CVER"] = "22";
        query["X-HTTP-Session-Id"] = "gsessionid";
        query["$httpHeaders"] = $"X-Goog-Api-Client:gl-js/ fire/8.10.0\nContent-Type:text/plain\nAuthorization:Bearer {AppState.IdToken}\n";
        query["t"] = "1";
        uriBuilder.Query = query.ToString();
        using var request = new HttpRequestMessage(HttpMethod.Post, uriBuilder.ToString()) {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>() {
                ["count"] = "1",
                ["ofs"] = "0",
                ["req___data__"] = JsonSerializer.Serialize(new {
                    database = $"projects/{ProjectId}/databases/(default)",
                    addTarget = new {
                        documents = new[] { documentPath },
                        targetId = targetId
                    }
                
                })
            })
        };
        var response = await Http.SendAsync(request);
        var sessionId = JsonSerializer.Deserialize<List<JsonElement>>((await response.Content.ReadAsStringAsync()).Split('\n')[1])![0][1][1].GetString()!;
        var gSessionId = response.Headers.GetValues("X-Http-Session-Id").First()!;
        throw new NotImplementedException();
    }

    public class ListeningChannel {
        private int RequestId;
        private int TargetId;
        private HttpClient Http;

        public ListeningChannel(HttpClient http, int requestId, int targetId) {
            Http = http;
            RequestId = requestId;
            TargetId = targetId;
        }

        public async Task Test() {
            throw new NotImplementedException();
            using var response = await Http.GetAsync("");
            var stream = await response.Content.ReadAsStreamAsync();
            var reader = new StreamReader(stream);
            while (!reader.EndOfStream) {
                var line = await reader.ReadLineAsync();
                Console.WriteLine($"NEW LINE: {line}");
            }
        }
    }
}