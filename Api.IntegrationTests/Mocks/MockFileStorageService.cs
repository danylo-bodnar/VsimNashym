using api.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Api.IntegrationTests.Mocks
{
    public class MockFileStorageService : IFileStorageService
    {
        public List<string> UploadedMessageIds { get; } = new();
        public List<string> DeletedMessageIds { get; } = new();

        public Task<(string url, string messageId)> UploadProfilePhotoAsync(IFormFile file)
        {
            var messageId = Guid.NewGuid().ToString();
            UploadedMessageIds.Add(messageId);

            var url = $"https://fake-storage.test/{file.FileName}";
            return Task.FromResult((url, messageId));
        }

        public Task DeleteProfilePhotoAsync(string messageId)
        {
            DeletedMessageIds.Add(messageId);
            return Task.CompletedTask;
        }
    }
}