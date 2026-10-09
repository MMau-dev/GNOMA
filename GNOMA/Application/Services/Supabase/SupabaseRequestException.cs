namespace GNOMA.Application.Services.Supabase
{
    public sealed class SupabaseRequestException : Exception
    {
        public int StatusCode { get; }

        public SupabaseRequestException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
