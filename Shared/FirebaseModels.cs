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
            Id = element.GetProperty("id").GetString()!;
            Team = element.TryGetProperty("team", out var team) ? team.GetInt32() : 0;
        }
    }

    public class _User {
        public string Id { get; init; }
        public string Name { get; init; }

        public _User(JsonElement element) {
            Id = element.GetProperty("id").GetString()!;
            Name = element.GetProperty("name").GetString()!;
        }
    }

    public class _Game {
        public int DiseasePlayerId { get; init; }
        public List<_Event> Events { get; init; }
        public bool IsOver { get; init; }
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
                EventName = element.GetProperty("action").GetString()!;
                Json = element;
            }
        }

        public class _Player {
            public int HP { get; init; }
            public int MP { get; init; }
            public int CP { get; init; }
            public int Id { get; init; }
            public int Team { get; init; }
            public List<_Item> Items { get; init; }
            public string Name { get; init; }
            public string UserId { get; init; }
            public List<string> Curses { get; init; }
            public string Guardian { get; set; }

            public class _Item {
                public int Id { get; init; }
                public int ModelId { get; init; }
                public bool? Used { get; set; }
                public int? FakeModelId { get; set; }

                public _Item(JsonElement element) {
                    if (!element.TryGetProperty("id", out var id)) {
                        Id = -1;
                        ModelId = -1;
                        return;
                    }
                    Id = id.GetInt32();
                    ModelId = element.GetProperty("modelId").GetInt32();
                    Used = element.TryGetProperty("used", out var used) ? used.GetBoolean() : null;
                    FakeModelId = element.TryGetProperty("fakeModelId", out var fakeModelId) ? fakeModelId.GetInt32() : 0;
                }

                public _Item() {
                    Id = 0;
                    ModelId = 0;
                }
            }

            public _Player(JsonElement element) {
                HP = element.TryGetProperty("hp", out var hp) ? hp.GetInt32() : 0;
                MP = element.TryGetProperty("mp", out var mp) ? mp.GetInt32() : 0;
                CP = element.TryGetProperty("cp", out var cp) ? cp.GetInt32() : 0;
                Id = element.GetProperty("id").GetInt32();
                Team = element.TryGetProperty("team", out var team) ? team.GetInt32() : 0;
                // Items = element.GetProperty("items").GetArrayEnumerator().Select(x => x.GetMap().TryGetFields(out var fields) ? new _Item(fields) : new _Item()).ToList();
                Items = element.GetProperty("items").EnumerateArray().Select(x => new _Item(x)).ToList();
                Name = element.GetProperty("name").GetString()!;
                UserId = element.GetProperty("userId").GetString()!;
                Curses = element.TryGetProperty("curses", out var curses) ? curses.EnumerateArray().Select(x => x.GetString()!).ToList() : [];
                Guardian = element.TryGetProperty("guardian", out var guardian) ? guardian.GetString()! : "";
            }

            public _Player() {
                HP = 0;
                MP = 0;
                CP = 0;
                Id = 0;
                Team = 0;
                Items = [];
                Name = "";
                UserId = "";
                Curses = [];
                Guardian = "";
            }
        }

        public _Game(JsonElement element) {
            DiseasePlayerId = element.TryGetProperty("diseasePlayerId", out var diseasePlayerId) ? diseasePlayerId.GetInt32() : 0;
            // Events = element.GetProperty("events").GetArrayEnumerator().Select(x => new _Event(x.GetMapFields())).ToList();
            Events = element.TryGetProperty("events", out var events) ? events.EnumerateArray().Select(x => new _Event(x)).ToList() : [];
            IsOver = element.TryGetProperty("isOver", out var isOver) && isOver.GetBoolean();
            GiftPlayerIds = element.TryGetProperty("giftPlayerIds", out var giftPlayerIds) ? giftPlayerIds.EnumerateArray().Select(x => x.GetInt32()).ToList() : [];
            GuardianPlayerIds = element.TryGetProperty("guardianPlayerIds", out var guardianPlayerIds) ? guardianPlayerIds.EnumerateArray().Select(x => x.GetInt32()).ToList() : [];
            Players = element.GetProperty("players").EnumerateArray().Select(x => new _Player(x)).ToList();
            TieBreakerTurnCount = element.GetProperty("tiebreakerTurnCount").GetInt32();
            TurnCount = element.GetProperty("turnCount").GetInt32();
            TurnPlayerId = element.GetProperty("turnPlayerId").GetInt32();
            UpdateCount = element.GetProperty("updateCount").GetInt32();
        }

        public _Game() {
            DiseasePlayerId = 0;
            Events = [];
            IsOver = false;
            GiftPlayerIds = [];
            GuardianPlayerIds = [];
            Players = [];
            TieBreakerTurnCount = 0;
            TurnCount = 0;
            TurnPlayerId = 0;
            UpdateCount = 0;
        }
    }

    public HiddenRoom(JsonDocument document) {
        Password = document.RootElement.GetProperty("password").GetString()!;
        Users = document.RootElement.GetProperty("users").EnumerateArray().Select(x => new _User(x)).ToList();
        if (document.RootElement.EnumerateObject().Any(x => x.NameEquals("entry"))) {
            var entry = document.RootElement.GetProperty("entry");
            Entries = entry.TryGetProperty("users", out var entries) ? entries.EnumerateArray().Select(x => new _UserEntry(x)).ToList() : [];
            TieBreakerTurnCount = entry.GetProperty("tiebreakerTurnCount").GetInt32();
        } else if (document.RootElement.EnumerateObject().Any(x => x.NameEquals("game"))) {
            Game = new(document.RootElement.GetProperty("game"));
            TieBreakerTurnCount = Game.TieBreakerTurnCount;
        }
    }

    public HiddenRoom() {
        Password = "";
        Users = [];
        Entries = [];
        Game = null;
        TieBreakerTurnCount = 0;
    }
}