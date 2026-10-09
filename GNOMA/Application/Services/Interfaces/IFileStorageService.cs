namespace GNOMA.Application.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(
            string accessToken,
            string objectPath,
            Stream content,
            string contentType,
            CancellationToken cancellationToken = default);
    }
}
