namespace GNOMA.Models.Auth
{
    public sealed record AuthResult(
        string AccessToken,
        string RefreshToken,
        string UserId,
        string Email
    );
}
