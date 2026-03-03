using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmsRazor.BLL.Services;

namespace SmsRazor.WebApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Ensure only logged in users can upload files
public class ChatFileController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;

    public ChatFileController(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    [HttpPost("Upload")]
    // [IgnoreAntiforgeryToken] if mixing API and Razor Pages without proper header setup in JS
    [IgnoreAntiforgeryToken]
    [RequestSizeLimit(524_288_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 524_288_000)]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file selected." });
        }

        try
        {
            // Set max file size to 500MB
            if (file.Length > 500L * 1024 * 1024)
            {
                return BadRequest(new { message = "File exceeds the 500MB limit." });
            }

            var containerName = "chat-attachments";
            var fileUrl = await _blobStorageService.UploadFileAsync(file, containerName);

            if (string.IsNullOrEmpty(fileUrl))
            {
                return StatusCode(500, new { message = "Failed to upload to blob storage." });
            }

            return Ok(new { fileUrl });
        }
        catch (Exception ex)
        {
            // Log error
            Console.WriteLine($"[ChatFileController] Upload Error: {ex.ToString()}");
            return StatusCode(500, new { message = "An internal error occurred during upload." });
        }
    }
}
