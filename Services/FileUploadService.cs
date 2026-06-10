namespace SentinelAI.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was uploaded. Please select a valid file.");

            if (file.Length > MaxFileSizeBytes)
                throw new ArgumentException($"File size exceeds the maximum limit of 10 MB. Your file is {file.Length / (1024 * 1024):F1} MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ArgumentException($"Invalid file type '{extension}'. Allowed types: {string.Join(", ", AllowedExtensions)}.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("File uploaded successfully: {FileName}", uniqueFileName);
            return $"/uploads/{uniqueFileName}";
        }

        public void DeleteFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            var fileName = Path.GetFileName(relativePath);
            var fullPath = Path.Combine(_environment.WebRootPath, "uploads", fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted: {FileName}", fileName);
            }
        }
    }
}
