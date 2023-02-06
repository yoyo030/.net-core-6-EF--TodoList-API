using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Todo.Services;
using Todo.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly TodoContext _todoContext;
        public FileUploadController(IWebHostEnvironment env, TodoContext todoContext)
        {
            _env = env;//注入根目錄
            _todoContext = todoContext;
        }
        [HttpPost]
        public void Post(ICollection<IFormFile> files)
        {
            var rootPath = _env.ContentRootPath + "\\wwwroot\\";
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = file.FileName;
                    using (var stream = System.IO.File.Create(rootPath + filePath))
                    {
                        file.CopyTo(stream);
                    }
                }
            }
        }

        [HttpPost("{id}")]
        public void Post(ICollection<IFormFile> files,Guid  id)
        {
            var rootPath = _env.ContentRootPath + $"\\wwwroot\\UploadFiles\\{id}\\";
            
            if(!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = file.FileName;
                    using (var stream = System.IO.File.Create(rootPath + fileName))
                    {
                        file.CopyTo(stream);

                        var insert = new UploadFile
                        {
                            Name = fileName,
                            Src = "/UploadFiles/" + id + "/" + fileName,
                            TodoId = id

                        };
                        _todoContext.UploadFiles.Add(insert);
                    }
                }
                _todoContext.SaveChanges(); 
            }
        }

        [HttpPost("PostFromForm")]
        public void PostFromForm(ICollection<IFormFile> files,[FromForm] Guid id)
        {
            var rootPath = _env.ContentRootPath + $"\\wwwroot\\UploadFiles\\{id}\\";

            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var fileName = file.FileName;
                    using (var stream = System.IO.File.Create(rootPath + fileName))
                    {
                        file.CopyTo(stream);

                        var insert = new UploadFile
                        {
                            Name = fileName,
                            Src = "/UploadFiles/" + id + "/" + fileName,
                            TodoId = id

                        };
                        _todoContext.UploadFiles.Add(insert);
                    }
                }
                _todoContext.SaveChanges();
            }
        }
    }
}
