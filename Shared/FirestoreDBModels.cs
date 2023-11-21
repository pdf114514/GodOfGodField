using System.Text.Json;

namespace GodOfGodField.Shared;

public class HiddenRoom {
    public int TieBreakerTurnCount { get; init; }
    public string Password { get; init; }
    public List<_User> Users { get; init; }
    public List<_UserEntry>? Entries { get; init; }
    public _Game? Game { get; set; }

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

    public class _Game {
        public int DiseasePlayerId { get; init; }
        public List<_Event> Events { get; init; }
        public List<int> GiftPlayerIds { get; init; }
        public List<int> GuardianPlayerIds { get; init; }
        public List<_Player> Players { get; init; }
        public int TieBreakerTurnCount { get; init; }
        public int TurnCount { get; init; }
        public int TurnPlayerId { get; init; }
        public int UpdateCount { get; init; }

        public class _Event {
            public string EventName { get; init; }
            public JsonElement Json { get; init; }

            public _Event(JsonElement element) {
                EventName = element.GetProperty("action").GetProperty("stringValue").GetString()!;
                Json = element;
            }
        }

        public class _Player {
            public int HP { get; init; }
            public int MP { get; init; }
            public int CP { get; init; }
            public int Id { get; init; }
            public int Team { get; init; }
            public List<_Event> Events { get; init; }
            public List<_Item> Items { get; init; }
            public string Name { get; init; }
            public string UserId { get; init; }

            public class _Item {
                public int Id { get; init; }
                public int ModelId { get; init; }

                public _Item(JsonElement element) {
                    Id = int.Parse(element.GetProperty("id").GetProperty("integerValue").GetString()!);
                    ModelId = int.Parse(element.GetProperty("modelId").GetProperty("integerValue").GetString()!);
                }
            }

            public _Player(JsonElement element) {
                HP = int.Parse(element.GetProperty("hp").GetProperty("integerValue").GetString()!);
                MP = int.Parse(element.GetProperty("mp").GetProperty("integerValue").GetString()!);
                CP = int.Parse(element.GetProperty("cp").GetProperty("integerValue").GetString()!);
                Id = int.Parse(element.GetProperty("id").GetProperty("integerValue").GetString()!);
                Team = element.TryGetProperty("team", out var team) ? int.Parse(team.GetProperty("integerValue").GetString()!) : 0;
                Events = element.GetProperty("events").GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => new _Event(x.GetProperty("mapValue").GetProperty("fields"))).ToList();
                Items = element.GetProperty("items").GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => new _Item(x.GetProperty("mapValue").GetProperty("fields"))).ToList();
                Name = element.GetProperty("name").GetProperty("stringValue").GetString()!;
                UserId = element.GetProperty("userId").GetProperty("stringValue").GetString()!;
            }
        }

        public _Game(JsonElement element) {
            DiseasePlayerId = int.Parse(element.GetProperty("diseasePlayerId").GetProperty("integerValue").GetString()!);
            GiftPlayerIds = element.GetProperty("giftPlayerIds").GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => int.Parse(x.GetProperty("integerValue").GetString()!)).ToList();
            GuardianPlayerIds = element.GetProperty("guardianPlayerIds").GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => int.Parse(x.GetProperty("integerValue").GetString()!)).ToList();
            Players = element.GetProperty("players").GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => new _Player(x.GetProperty("mapValue").GetProperty("fields"))).ToList();
            TieBreakerTurnCount = int.Parse(element.GetProperty("tiebreakerTurnCount").GetProperty("integerValue").GetString()!);
            TurnCount = int.Parse(element.GetProperty("turnCount").GetProperty("integerValue").GetString()!);
            TurnPlayerId = int.Parse(element.GetProperty("turnPlayerId").GetProperty("integerValue").GetString()!);
            UpdateCount = int.Parse(element.GetProperty("updateCount").GetProperty("integerValue").GetString()!);
        }
    }

    public HiddenRoom(JsonDocument document) {
        Password = document.RootElement.GetProperty("password").GetProperty("stringValue").GetString()!;
        var users = document.RootElement.GetProperty("users");
        Users = users.GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => new _User(x.GetProperty("mapValue").GetProperty("fields"))).ToList();
        if (document.RootElement.EnumerateObject().Any(x => x.NameEquals("entry"))) {
            var entry = document.RootElement.GetProperty("entry").GetProperty("mapValue").GetProperty("fields");
            Entries = entry.TryGetProperty("users", out var entries) ? entries.GetProperty("arrayValue").GetProperty("values").EnumerateArray().Select(x => new _UserEntry(x.GetProperty("mapValue").GetProperty("fields"))).ToList() : [];
            TieBreakerTurnCount = int.Parse(entry.GetProperty("tiebreakerTurnCount").GetProperty("integerValue").GetString()!);
        } else if (document.RootElement.EnumerateObject().Any(x => x.NameEquals("game"))) {
            Game = new(document.RootElement.GetProperty("game").GetProperty("mapValue").GetProperty("fields"));
            TieBreakerTurnCount = Game.TieBreakerTurnCount;
        }
    }
}