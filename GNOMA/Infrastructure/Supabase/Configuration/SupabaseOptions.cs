namespace GNOMA.Infrastructure.Supabase.Configuration
{
    public sealed class SupabaseOptions
    {
        public const string SectionName = "Supabase";

        public string Url { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public string StorageBucket { get; set; } = string.Empty;
    }
}
