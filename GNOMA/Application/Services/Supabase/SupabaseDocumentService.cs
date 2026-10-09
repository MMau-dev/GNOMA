using GNOMA.Application.Services.Interfaces;
using GNOMA.Infrastructure.Supabase.Configuration;
using GNOMA.Models.Documents;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace GNOMA.Application.Services.Supabase
{
    public sealed class SupabaseDocumentService : IDocumentService
    {
        private readonly HttpClient _httpClient;
        private readonly SupabaseOptions _options;

        public SupabaseDocumentService(
            HttpClient httpClient,
            IOptions<SupabaseOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<DocumentDto>> GetMineAsync(
            string accessToken,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(
                HttpMethod.Get,
                "documents?select=id,owner_id,name,storage_path,created_at&order=created_at.desc",
                accessToken);

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);

            var documents = await response.Content
                .ReadFromJsonAsync<List<DocumentDto>>(
                    cancellationToken: cancellationToken);

            return documents ?? [];
        }

        public async Task<DocumentDto> CreateAsync(
            string accessToken,
            string name,
            string storagePath,
            CancellationToken cancellationToken = default)
        {
            using var request = CreateRequest(
                HttpMethod.Post,
                "documents",
                accessToken);

            request.Headers.TryAddWithoutValidation(
                "Prefer",
                "return=representation");

            request.Content = JsonContent.Create(new
            {
                name,
                storage_path = storagePath
            });

            using var response = await _httpClient.SendAsync(
                request,
                cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);

            var documents = await response.Content
                .ReadFromJsonAsync<List<DocumentDto>>(
                    cancellationToken: cancellationToken);

            return documents?.FirstOrDefault()
                ?? throw new InvalidOperationException(
                    "Supabase no devolvió el documento creado.");
        }

        private static HttpRequestMessage CreateRequest(
            HttpMethod method,
            string endpoint,
            string accessToken)
        {
            var request = new HttpRequestMessage(method, endpoint);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            return request;
        }

        private static async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            CancellationToken cancellationToken)
        {
            if (response.IsSuccessStatusCode)
                return;

            // No registrar el cuerpo de error, porque puede contener datos
            // de la petición o información que no debe filtrarse.
            throw new SupabaseRequestException(
                (int)response.StatusCode,
                "La operación de base de datos no pudo completarse.");
        }
    }
}
