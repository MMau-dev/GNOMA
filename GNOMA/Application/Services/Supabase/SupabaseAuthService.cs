using GNOMA.Application.Services.Interfaces;
using GNOMA.Infrastructure.Supabase.Configuration;
using GNOMA.Models.Auth;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace GNOMA.Application.Services.Supabase
{
    public sealed class SupabaseAuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly SupabaseOptions _options;

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        public SupabaseAuthService(
            HttpClient httpClient,
            IOptions<SupabaseOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public Task<AuthResult> SignInAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            return AuthenticateAsync(
                "token?grant_type=password",
                email,
                password,
                cancellationToken);
        }

        public Task<AuthResult> SignUpAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            return AuthenticateAsync(
                "signup",
                email,
                password,
                cancellationToken);
        }

        private async Task<AuthResult> AuthenticateAsync(
            string endpoint,
            string email,
            string password,
            CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint);

            request.Content = JsonContent.Create(new
            {
                email,
                password
            });

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            var content = await response.Content.ReadAsStringAsync(
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new SupabaseRequestException(
                    (int)response.StatusCode,
                    "No fue posible completar la autenticación.");
            }

            using var json = JsonDocument.Parse(content);
            var root = json.RootElement;

            // Supabase puede crear la cuenta sin una sesión activa
            // cuando se requiere confirmar el correo electrónico.
            if (!root.TryGetProperty("access_token", out var accessToken) ||
                !root.TryGetProperty("refresh_token", out var refreshToken) ||
                !root.TryGetProperty("user", out var user))
            {
                throw new InvalidOperationException(
                    "La cuenta fue procesada, pero no hay una sesión activa. " +
                    "Verifica el correo electrónico si se requiere confirmación.");
            }

            var userId = user.GetProperty("id").GetString();
            var userEmail = user.TryGetProperty("email", out var emailElement)
                ? emailElement.GetString()
                : email;

            if (string.IsNullOrWhiteSpace(userId) ||
                string.IsNullOrWhiteSpace(userEmail))
            {
                throw new InvalidOperationException(
                    "Supabase devolvió datos de usuario incompletos.");
            }

            return new AuthResult(
                accessToken.GetString()!,
                refreshToken.GetString()!,
                userId,
                userEmail);
        }

        public async Task SignOutAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "logout");

            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    accessToken);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            // El cierre local de sesión también debe ejecutarse
            // aunque el endpoint remoto no esté disponible.
            if (!response.IsSuccessStatusCode)
            {
                throw new SupabaseRequestException(
                    (int)response.StatusCode,
                    "No fue posible invalidar la sesión remota.");
            }
        }
    }
}
