using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using hello_webapi.Models;

namespace hello_webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private IHostingEnvironment hostingEnvironment;
        public FilesController(IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [Route("/api/files/upload")]
        public JsonResult Upload()
        {
            var files = Request.Form.Files.Where(f => f.Length > 0);
            var totalSize = files.Sum(f => f.Length);
            var rootPath = hostingEnvironment.WebRootPath;
            Console.WriteLine(rootPath);
            var fileEntries = new List<FileEntry>();
            foreach (var file in files)
            {
                var fileId = Guid.NewGuid().ToString("N");
                var filePath = Path.Combine("Upload", fileId, file.FileName);
                var fileName = Path.Combine(rootPath, filePath);
                var uploadPath = Path.GetDirectoryName(fileName);
                if(!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);
                using (var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    file.CopyToAsync(fileStream);
                }

                fileEntries.Add(new FileEntry()
                {
                    FileId = fileId,
                    FileName = file.FileName,
                    FileSize = file.Length,
                    AssetURL = $@"/{filePath}".Replace("\\","/"),
                    CreateTime = DateTime.Now
                });
            }

            return new JsonResult(fileEntries);
        }

        [HttpGet]
        [Route("/api/files/download/{fileId}")]
        public IActionResult Download(string fileId)
        {
            if(string.IsNullOrEmpty(fileId))
                return BadRequest("fileId can't be null or empty");
                
            var rootPath = hostingEnvironment.WebRootPath;
            var uploadPath = Path.Combine(rootPath, "Upload",fileId);
            if(!Directory.Exists(uploadPath)) return BadRequest("invalid fileId to request asset");
            var files = Directory.GetFiles(uploadPath);
            if(files.Length<=0) return BadRequest("There is not file found for your request");

            var fileName = files[0];
            var fileStream = new FileStream(fileName,FileMode.Open);
            var fileExt = Path.GetExtension(fileName);
            var memiType = new FileExtensionContentTypeProvider().Mappings[fileExt];
            return File(fileStream,memiType);
        }
    }
}