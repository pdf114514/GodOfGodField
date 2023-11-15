using K = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace GodOfGodField.Shared;

public class SignUpResponse {
    [K("expiresIn")] public required string ExpiresIn { get; init; }
    [K("idToken")] public required string IdToken { get; init; }
    [K("kind")] public required string Kind { get; init; }
    [K("localId")] public required string LocalId { get; init; }
    [K("refreshToken")] public required string RefreshToken { get; init; }
}

public class GetAccountInfoRequest {
    [K("idToken")] public required string IdToken { get; init; }
}

public class AccountInfo {
    [K("kind")] public required string Kind { get; init; }
    [K("users")] public required _User[] Users { get; init; }

    public class _User {
        [K("createdAt")] public required string CreatedAt { get; init; }
        [K("lastLoginAt")] public required string LastLoginAt { get; init; }
        [K("lastRefreshAt")] public required string LastRefreshAt { get; init; }
        [K("localId")] public required string LocalId { get; init; }
    }
}

public class RefreshTokenRequest {
    [K("refreshToken")] public required string RefreshToken { get; init; }
}

public class RefreshTokenResponse {
    [K("access_token")] public required string AccessToken { get; init; }
    [K("expires_in")] public required string ExpiresIn { get; init; }
    [K("id_token")] public required string IdToken { get; init; }
    [K("project_id")] public required string ProjectId { get; init; }
    [K("refresh_token")] public required string RefreshToken { get; init; }
    [K("token_type")] public required string TokenType { get; init; }
    [K("user_id")] public required string UserId { get; init; }
}

public class GetGFSessionRequest {
    [K("count")] public required int Count { get; init; }
    [K("ofs")] public required int Ofs { get; init; }
    [K("req0__data__")] public required string Req0Data { get; init; }
}

public class GFSession {
    [K("sessionId")] public required string SessionId { get; init; }
    [K("gSessionId")] public required string GSessionId { get; init; }
}

public class UserCount {
    [K("training")] public required int Training { get; init; }
    [K("hidden")] public required int Hidden { get; init; }
    [K("duel")] public required int Duel { get; init; }
}

public class DuelRecord {
    [K("rating")] public required int Rating { get; init; }
    [K("gameCount")] public required int GameCount { get; init; }
    [K("enemyUserIds")] public required string[] EnemyUserIds { get; init; }
}

public class AddDuelUserRequest {
    [K("lang")] public required string Lang { get; init; }
    [K("mode")] public string Mode { get; init; } = "duel";
    [K("userName")] public required string UserName { get; init; }
}

public class CreateRoomRequest {
    [K("mode")] public string Mode { get; init; } = "hidden";
    [K("password")] public required string Password { get; init; }
    [K("userName")] public required string UserName { get; init; }
}

public class CreateRoomResponse {
    [K("roomId")] public required string RoomId { get; init; }
}

public class AddRoomUserRequest {
    [K("mode")] public string Mode { get; init; } = "hidden";
    [K("roomId")] public required string RoomId { get; init; }
    [K("userName")] public required string UserName { get; init; }
}

public class RemoveRoomUserRequest {
    [K("mode")] public string Mode { get; init; } = "hidden";
    [K("roomId")] public required string RoomId { get; init; }
}