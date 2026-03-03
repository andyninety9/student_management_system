using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace SmsRazor.BLL.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly string _connectionString;

    public BlobStorageService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("AzureBlobStorage") ?? "UseDevelopmentStorage=true";
    }

    public async Task<string> UploadFileAsync(IFormFile file, string containerName)
    {
        if (file == null || file.Length == 0) return string.Empty;

        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // Create container if it doesn't exist
        try 
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
        {
            // Fallback to Private container if Public Access is disabled on the Storage Account
            await blobContainerClient.CreateIfNotExistsAsync();
        }

        var extension = Path.GetExtension(file.FileName);
        // Clean filename to prevent weird characters breaking the URL
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
        var cleanFileName = new string(fileNameWithoutExtension.Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
        var uniqueFileName = $"{Guid.NewGuid()}_{cleanFileName}{extension}";

        var blobClient = blobContainerClient.GetBlobClient(uniqueFileName);

        var blobHttpHeader = new BlobHttpHeaders { ContentType = file.ContentType };

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = blobHttpHeader });

        if (blobClient.CanGenerateSasUri)
        {
            var sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = blobContainerClient.Name,
                BlobName = blobClient.Name,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddYears(100)
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            return blobClient.GenerateSasUri(sasBuilder).ToString();
        }

        return blobClient.Uri.ToString();
    }

    public async Task<bool> DeleteFileAsync(string fileUrl, string containerName)
    {
        if (string.IsNullOrEmpty(fileUrl)) return false;

        var blobServiceClient = new BlobServiceClient(_connectionString);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var uri = new Uri(fileUrl);
        var blobName = Path.GetFileName(uri.LocalPath);
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        var response = await blobClient.DeleteIfExistsAsync();
        return response.Value;
    }
}
