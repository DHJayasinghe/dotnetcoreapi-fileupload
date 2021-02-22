using Microsoft.AspNetCore.Http;

namespace DotnetCoreApi.FileUpload.Models
{
    public sealed class SimpleFileUploadModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IFormFile Somefile { get; set; }
    }
}
