using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider;
        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            this.fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
                ?? throw new ArgumentException(nameof(fileExtensionContentTypeProvider));
        }
        [HttpGet("{fileId}")]
        public ActionResult GetFile(string fileId)
        {
            var mockFilePath = "CityInfo.postman_collection.json";
            if (!System.IO.File.Exists(mockFilePath))
                return NotFound();
            
            if (!fileExtensionContentTypeProvider.TryGetContentType(mockFilePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = System.IO.File.ReadAllBytes(mockFilePath);
            return File(bytes, contentType, Path.GetFileName(mockFilePath));
        }
    }
}
