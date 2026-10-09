using GNOMA.Application.Services.Interfaces;
using GNOMA.Application.Services.Supabase;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GNOMA.Controllers.Dashboard
{
    [Authorize]
    public sealed class DocumentsController : Controller
    {
        private const long MaxFileSize = 10 * 1024 * 1024;

        private readonly IDocumentService _documentService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(
            IDocumentService documentService,
            IFileStorageService fileStorageService,
            ILogger<DocumentsController> logger)
        {
            _documentService = documentService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var token = GetAccessToken();

            if (token is null)
                return Challenge();

            try
            {
                var documents = await _documentService.GetMineAsync(
                    token,
                    cancellationToken);

                return View(documents);
            }
            catch (SupabaseRequestException ex)
            {
                _logger.LogWarning(
                    "Error consultando documentos. HTTP {StatusCode}.",
                    ex.StatusCode);

                return Problem(
                    title: "No fue posible cargar los documentos.",
                    statusCode: StatusCodes.Status502BadGateway);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
        public async Task<IActionResult> Upload(
            IFormFile? file,
            CancellationToken cancellationToken)
        {
            if (file is null || file.Length == 0)
            {
                TempData["Error"] = "Selecciona un archivo.";
                return RedirectToAction(nameof(Index));
            }

            if (file.Length > MaxFileSize)
            {
                TempData["Error"] = "El tamaño máximo es de 10 MB.";
                return RedirectToAction(nameof(Index));
            }

            // Lista de ejemplo: ajústala a los tipos que tu negocio necesite.
            var allowedTypes = new Dictionary<string, string>(
                StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = "application/pdf",
                [".png"] = "image/png",
                [".jpg"] = "image/jpeg",
                [".jpeg"] = "image/jpeg"
            };

            var extension = Path.GetExtension(file.FileName);

            if (!allowedTypes.TryGetValue(extension, out var contentType))
            {
                TempData["Error"] = "Tipo de archivo no permitido.";
                return RedirectToAction(nameof(Index));
            }

            var token = GetAccessToken();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (token is null || !Guid.TryParse(userId, out var ownerId))
                return Challenge();

            // Nunca usamos el nombre enviado por el usuario como ruta.
            var objectPath = $"{ownerId:D}/{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

            try
            {
                await using var stream = file.OpenReadStream();

                var storagePath = await _fileStorageService.UploadAsync(
                    token,
                    objectPath,
                    stream,
                    contentType,
                    cancellationToken);

                await _documentService.CreateAsync(
                    token,
                    Path.GetFileName(file.FileName),
                    storagePath,
                    cancellationToken);

                TempData["Success"] = "Documento subido correctamente.";
            }
            catch (SupabaseRequestException ex)
            {
                _logger.LogWarning(
                    "Error al subir o registrar un documento. HTTP {StatusCode}.",
                    ex.StatusCode);

                TempData["Error"] =
                    "No fue posible guardar el documento.";
            }

            return RedirectToAction(nameof(Index));
        }

        private string? GetAccessToken() =>
            User.FindFirstValue("supabase_access_token");
    }
}
