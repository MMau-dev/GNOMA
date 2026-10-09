using GNOMA.Models.Documents;

namespace GNOMA.Application.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<IReadOnlyList<DocumentDto>> GetMineAsync(
            string accessToken,
            CancellationToken cancellationToken = default);

        Task<DocumentDto> CreateAsync(
            string accessToken,
            string name,
            string storagePath,
            CancellationToken cancellationToken = default);
    }
}
