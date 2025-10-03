
namespace api.Interfaces
{
    public interface IFileStorageService
    {
        Task<(string url, string messageId)> UploadProfilePhotoAsync(IFormFile file);
        Task DeleteProfilePhotoAsync(string messageId);
    }
}