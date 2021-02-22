namespace DotnetCoreApi.FileUpload.Controllers
{
    public sealed class FileUploadResult
    {
        public bool IsSuccess { get; }
        public string Filename { get; set; }
        public string Error { get; }

        private FileUploadResult(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static FileUploadResult Success(string filename) =>
            new FileUploadResult(true, null)
            {
                Filename = filename
            };

        public static FileUploadResult Failure(string error) =>
            new FileUploadResult(false, error);
    }
}
