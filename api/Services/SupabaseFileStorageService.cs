using api.Interfaces;

namespace api.Services
{
    public class SupabaseFileStorageService : IFileStorageService
    {
        private readonly Supabase.Client _client;
        private readonly string _bucketName;
        private readonly ILogger<SupabaseFileStorageService> _logger;

        public SupabaseFileStorageService(string supabaseUrl, string supabaseKey, string bucketName, ILogger<SupabaseFileStorageService> logger)
        {
            _bucketName = bucketName;
            _client = new Supabase.Client(supabaseUrl, supabaseKey);
            _client.InitializeAsync().GetAwaiter().GetResult();
            _logger = logger;
        }

        public async Task<(string url, string messageId)> UploadProfilePhotoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty or null", nameof(file));

            try
            {
                var storage = _client.Storage.From(_bucketName);

                var fileName = $"{Guid.NewGuid()}-{file.FileName}";

                byte[] fileBytes;
                await using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                    fileBytes = ms.ToArray();
                }

                await storage.Upload(fileBytes, fileName);

                var url = storage.GetPublicUrl(fileName);

                _logger.LogInformation("Successfully uploaded file '{FileName}' to bucket '{BucketName}'.", fileName, _bucketName);

                return (url, fileName);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while uploading file '{FileName}' to Supabase.", file.FileName);
                throw new InvalidOperationException("Unable to connect to Supabase. Check network or Supabase URL configuration.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error uploading file '{FileName}' to bucket '{BucketName}'.", file?.FileName, _bucketName);
                throw new InvalidOperationException("An unexpected error occurred while uploading the profile photo.", ex);
            }
        }

        public async Task DeleteProfilePhotoAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be empty", nameof(fileName));

            try
            {
                var storage = _client.Storage.From(_bucketName);
                await storage.Remove(fileName);
                _logger.LogInformation("Successfully deleted file '{FileName}' from bucket '{BucketName}'.", fileName, _bucketName);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error while connecting to Supabase for file '{FileName}'.", fileName);
                throw new InvalidOperationException("Unable to connect to Supabase. Check network or Supabase URL configuration.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting file '{FileName}' from '{BucketName}'.", fileName, _bucketName);
                throw new InvalidOperationException("An unexpected error occurred while deleting the profile photo.", ex);
            }
        }
    }
}
