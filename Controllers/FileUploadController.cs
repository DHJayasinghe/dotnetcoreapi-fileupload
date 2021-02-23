using DotnetCoreApi.FileUpload.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetCoreApi.FileUpload.Controllers
{
    [ApiController]
    [Route("api/fileupload")]
    public sealed class FileUploadController : ControllerBase
    {
        private readonly string _fileSaveDirectory;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileUploadController> _logger;

        public FileUploadController(
            IWebHostEnvironment webHostEnvironment,
            ILogger<FileUploadController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));

            _fileSaveDirectory = $"{_webHostEnvironment.ContentRootPath}/AppData/Uploads";
            if (!Directory.Exists(_fileSaveDirectory)) // create directory path, if not exist
                Directory.CreateDirectory(_fileSaveDirectory);
        }

        [HttpPost("justfile")]
        public async Task<IActionResult> JustFile([FromForm] IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
                return BadRequest("Uploading file is not provided");

            var result = await WriteFilesToDiskAsync(
               formFile: formFile,
               directoryToSave: _fileSaveDirectory,
               logger: _logger);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Filename);
        }

        [HttpPost("using/model/simple")]
        public async Task<IActionResult> SimpleUpload([FromForm] SimpleFileUploadModel model)
        {
            if (model.Somefile == null || model.Somefile.Length == 0)
                return BadRequest("Uploading file is not provided");

            var result = await WriteFilesToDiskAsync(
                formFile: model.Somefile,
                directoryToSave: _fileSaveDirectory,
                logger: _logger);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            // DO something with model data. Ex: Save to DB with filename

            return Ok(new
            {
                model.Id,
                model.Name,
                Filename = result.Filename
            });
        }

        [HttpPost("using/model/complex")]
        public async Task<IActionResult> ComplexUpload([FromForm] ComplexFileUploadModel model)
        {
            if (model.Files == null || model.Files.Count == 0 || !model.Files.Any(d => d.Somefile != null))
                return BadRequest("Uploading file is not provided");

            var fileUploadTasks = new List<Task<FileUploadResult>>();
            model.Files.ForEach(data =>
           {
               fileUploadTasks.Add(WriteFilesToDiskAsync(
                formFile: data.Somefile,
                directoryToSave: _fileSaveDirectory,
                logger: _logger));
           });

            // DO something with model data. Ex: Save to DB with filename

            var result = await Task.WhenAll(fileUploadTasks);

            return Ok(new
            {
                Success = result.ToList().Count(d => d.IsSuccess),
                Failed = result.ToList().Count(d => !d.IsSuccess),
                UploadedFiles = result.ToList().Where(d => d.IsSuccess).Select(d => d.Filename)
            });
        }

        private byte[] ConvertToByteArray(IFormFile file)
        {
            if (file == null)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private async Task<FileUploadResult> WriteFilesToDiskAsync(
            IFormFile formFile,
            string directoryToSave,
            ILogger logger)
        {
            // Don't rely on or trust the FileName property without validation.
            // Instead generate unique filename ourself, using original filename extension
            string filename = GenerateFilename(Path.GetExtension(formFile.FileName));
            byte[] filecontent = ConvertToByteArray(formFile);
            try
            {
                await System.IO.File.WriteAllBytesAsync(Path.Combine(directoryToSave, filename), filecontent);
                return FileUploadResult.Success(filename);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occured while saving file to disk");
                return FileUploadResult.Failure(ex.Message);
            }

        }

        /// <summary>
        /// Generate an unique filename
        /// </summary>
        private string GenerateFilename(string extension) => $"{Guid.NewGuid().ToString().Replace("-", "")}{extension}";
    }
}
