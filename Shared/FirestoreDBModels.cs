using System.Text.Json;

namespace GodOfGodField.Shared;

public class HiddenRoom {
    public List<_UserEntry> Entries { get; init; }
    public int TieBreakerTurnCount { get; init; }
    public string Password { get; init; }
    public List<_User> Users { get; init; }

    public class _UserEntry {
        public string Id { get; init; }
        public int Team { get; init; }

        public _UserEntry(JsonElement element) {
            Id = element.GetProperty("id").GetProperty("stringValue").GetString()!;
            Team = element.TryGetProperty("team", out var team) ? int.Parse(team.GetProperty("integerValue").GetString()!) : 0;
        }
    }

    public class _User {
        public string Id { get; init; }
        public string Name { get; init; }

        public _User(JsonElement element) {
            Id = element.GetProperty("id").GetProperty("stringValue").GetString()!;
            Name = element.GetProperty("name").GetProperty("stringValue").GetString()!;
        }
    }

    public HiddenRoom(JsonDocument document) {
        var entry = document.RootElement.GetProperty("entry").GetProperty("mapValue").GetProperty("fields");
        Entries = entry.TryGetProperty("users", out var entries) ? entries.GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => new _UserEntry(x.GetProperty("mapValue").GetProperty("fields"))).ToList() : [];
        TieBreakerTurnCount = int.Parse(entry.GetProperty("tiebreakerTurnCount").GetProperty("integerValue").GetString()!);
        Password = document.RootElement.GetProperty("password").GetProperty("stringValue").GetString()!;
        var users = document.RootElement.GetProperty("users");
        Users = users.GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => new _User(x.GetProperty("mapValue").GetProperty("fields"))).ToList();
    }
}