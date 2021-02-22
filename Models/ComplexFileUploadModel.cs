using System.Collections.Generic;

namespace DotnetCoreApi.FileUpload.Models
{
    public sealed class ComplexFileUploadModel
    {
        public int Id { get; set; }
        public List<SimpleFileUploadModel> Files { get; set; }
    }
}
