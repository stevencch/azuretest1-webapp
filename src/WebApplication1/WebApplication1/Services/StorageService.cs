using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace WebApplication1.Services
{
    public class StorageService
    {
        private readonly string _connectionString = "";
        private readonly string _blobContainerName = "devtest1";
        public StorageService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("StorageService");
        }

        public async Task UploadFile(string fileName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_connectionString);
            BlobContainerClient blobContainerClient =
          blobServiceClient.GetBlobContainerClient(_blobContainerName);
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            // 2. Upload a Blob
            BlobClient blobClient = blobContainerClient.GetBlobClient(fileName);

            using FileStream fileStream = File.OpenRead(Path.Combine("uploads", fileName));

            await blobClient.UploadAsync(fileStream,
        new BlobHttpHeaders { ContentType = "text/html" });
        }
    }
}
