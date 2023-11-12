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