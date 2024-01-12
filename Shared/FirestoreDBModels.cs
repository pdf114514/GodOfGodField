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
            Id = element.GetProperty("id").GetStringValue();
            Team = element.TryGetProperty("team", out var team) ? team.GetIntValue() : 0;
        }
    }

    public class _User {
        public string Id { get; init; }
        public string Name { get; init; }

        public _User(JsonElement element) {
            Id = element.GetProperty("id").GetStringValue();
            Name = element.GetProperty("name").GetStringValue();
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
                EventName = element.GetProperty("action").GetStringValue();
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

                public _Item(JsonElement element) {
                    Id = element.GetProperty("id").GetIntValue();
                    ModelId = element.GetProperty("modelId").GetIntValue();
                }

                public _Item() {
                    Id = 0;
                    ModelId = 0;
                }
            }

            public _Player(JsonElement element) {
                HP = element.TryGetProperty("hp", out var hp) ? hp.GetIntValue() : 0;
                MP = element.TryGetProperty("mp", out var mp) ? mp.GetIntValue() : 0;
                CP = element.TryGetProperty("cp", out var cp) ? cp.GetIntValue() : 0;
                Id = element.GetProperty("id").GetIntValue();
                Team = element.TryGetProperty("team", out var team) ? team.GetIntValue() : 0;
                Items = element.GetProperty("items").GetArrayEnumerator().Select(x => x.GetMapValue().TryGetFieldsValue(out var fields) ? new _Item(fields) : new _Item()).ToList();
                Name = element.GetProperty("name").GetStringValue();
                UserId = element.GetProperty("userId").GetStringValue();
                Curses = element.TryGetProperty("curses", out var curses) ? curses.GetArrayEnumerator().Select(x => x.GetStringValue()).ToList() : [];
                Guardian = element.TryGetProperty("guardian", out var guardian) ? guardian.GetStringValue() : "";
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
            DiseasePlayerId = element.TryGetProperty("diseasePlayerId", out var diseasePlayerId) ? diseasePlayerId.GetIntValue() : 0;
            Events = element.GetProperty("events").GetArrayEnumerator().Select(x => new _Event(x.GetMapFieldsValue())).ToList();
            GiftPlayerIds = element.TryGetProperty("giftPlayerIds", out var giftPlayerIds) ? giftPlayerIds.GetArrayEnumerator().Select(x => x.GetIntValue()).ToList() : [];
            GuardianPlayerIds = element.TryGetProperty("guardianPlayerIds", out var guardianPlayerIds) ? guardianPlayerIds.GetArrayEnumerator().Select(x => x.GetIntValue()).ToList() : [];
            Players = element.GetProperty("players").GetArrayEnumerator().Select(x => new _Player(x.GetMapFieldsValue())).ToList();
            TieBreakerTurnCount = element.GetProperty("tiebreakerTurnCount").GetIntValue();
            TurnCount = element.GetProperty("turnCount").GetIntValue();
            TurnPlayerId = element.GetProperty("turnPlayerId").GetIntValue();
            UpdateCount = element.GetProperty("updateCount").GetIntValue();
        }

        public _Game() {
            DiseasePlayerId = 0;
            Events = [];
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
        Password = document.RootElement.GetProperty("password").GetStringValue();
        Users = document.RootElement.GetProperty("users").GetArrayEnumerator().Select(x => new _User(x.GetMapFieldsValue())).ToList();
        if (document.RootElement.EnumerateObject().Any(x => x.NameEquals("entry"))) {
            var entry = document.RootElement.GetProperty("entry").GetMapFieldsValue();
            Entries = entry.TryGetProperty("users", out var entries) ? entries.GetArrayEnumerator().Select(x => new _UserEntry(x.GetMapFieldsValue())).ToList() : [];
            TieBreakerTurnCount = entry.GetProperty("tiebreakerTurnCount").GetIntValue();
        } else if (document.RootElement.EnumerateObject().Any(x => x.NameEquals("game"))) {
            Game = new(document.RootElement.GetProperty("game").GetMapFieldsValue());
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