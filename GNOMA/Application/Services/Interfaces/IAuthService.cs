using GNOMA.Models.Auth;

namespace GNOMA.Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> SignInAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default);

        Task<AuthResult> SignUpAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default);

        Task SignOutAsync(
            string accessToken,
            CancellationToken cancellationToken = default);
    }
}
