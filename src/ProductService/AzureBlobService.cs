// ...existing code above...
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using System;

namespace ProductService
{
    public class AzureBlobService
    {
        private readonly string _connectionString;
        private readonly string _containerName;
        private readonly string _blobEndpoint;

        public AzureBlobService(IConfiguration config)
        {
            _connectionString = config["AzureBlob:ConnectionString"] ?? throw new InvalidOperationException("AzureBlob:ConnectionString missing");
            _containerName = config["AzureBlob:ContainerName"] ?? throw new InvalidOperationException("AzureBlob:ContainerName missing");
            _blobEndpoint = config["AzureBlob:BlobEndpoint"] ?? string.Empty;
        }

        public string GetBlobSasUrl(string blobName, int expiryMinutes = 60)
        {
            var blobClient = new BlobClient(_connectionString, _containerName, blobName);
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = _containerName,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            var sas = blobClient.GenerateSasUri(sasBuilder);
            return sas.ToString();
        }

        public BlobClient GetBlobClient(string blobName)
        {
            return new BlobClient(_connectionString, _containerName, blobName);
        }
    }
}
