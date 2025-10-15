using Microsoft.AspNetCore.Http;

namespace StylePoint.Application.Services.Interfaces;

public interface ICloudService
{
    Task<string> UploadImageAsync(IFormFile file);
    Task<string> UploadTrackAsync(IFormFile file);
}
