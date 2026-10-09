using System.Text.Json.Serialization;

namespace GNOMA.Models.Documents
{
    public sealed class DocumentDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("owner_id")]
        public Guid OwnerId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("storage_path")]
        public string StoragePath { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
