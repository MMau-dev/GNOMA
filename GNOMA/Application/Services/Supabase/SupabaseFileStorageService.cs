using GNOMA.Application.Services.Interfaces;
using GNOMA.Infrastructure.Supabase.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace GNOMA.Application.Services.Supabase
{
    public sealed class SupabaseFileStorageService : IFileStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly SupabaseOptions _options;

        public SupabaseFileStorageService(
            HttpClient httpClient,
            IOptions<SupabaseOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> UploadAsync(
            string accessToken,
            string objectPath,
            Stream content,
            string contentType,
            CancellationToken cancellationToken = default)
        {
            var encodedPath = string.Join(
                "/",
                objectPath.Split('/')
                    .Select(Uri.EscapeDataString));

            var endpoint =
                $"{Uri.EscapeDataString(_options.StorageBucket)}/{encodedPath}";

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            request.Headers.TryAddWithoutValidation(
                "x-upsert",
                "false");

            var streamContent = new StreamContent(content);
            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(contentType);

            request.Content = streamContent;

            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw new SupabaseRequestException(
                    (int)response.StatusCode,
                    "No fue posible subir el archivo.");
            }

            return objectPath;
        }
    }
}
