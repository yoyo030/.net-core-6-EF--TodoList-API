using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Todo.Models;
using Todo.Dtos;
using Todo.Parameters;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Todo.Controllers
{
    [Route("api/Todo/{TodoId}/UploadFile")]
    [ApiController]//這行會誘發物件資料驗證,WebMVC不會有這行,所以要在function內寫
    //if(!ModelState.IsValid)則...
    public class TodoUploadFileController : ControllerBase
    {
        private readonly TodoContext _todoContext;
     
        public TodoUploadFileController(TodoContext todoContext)
        {
            _todoContext = todoContext;
        }
        [HttpGet]
        public ActionResult<IEnumerable<UploadFileDto>> Get(Guid TodoId)
        {

            if (!_todoContext.TodoLists.Any(a => a.TodoId == TodoId))
            {
                return NotFound("找不到該事項");
            }

            var result = from a in _todoContext.UploadFiles
                         where a.TodoId == TodoId
                         select new UploadFileDto
                         {
                             Name = a.Name,
                             Src = a.Src,
                             TodoId = a.TodoId,
                             UploadFileId = a.UploadFileId
                         };

            if (result == null || result.Count() == 0)
            {
                return NotFound("找不到檔案");
            }
            return Ok(result);
        }

        // GET api/<TodoUploadFileController>/5
        [HttpGet("{UploadFileId}")]
        public ActionResult<UploadFileDto> Get(Guid TodoId, Guid UploadFileId)
        {
            if (!_todoContext.TodoLists.Any(a => a.TodoId == TodoId))
            {
                return NotFound("找不到該事項");
            }

            var result = (from a in _todoContext.UploadFiles
                         where a.TodoId == TodoId
                         && a.UploadFileId == UploadFileId
                         select new UploadFileDto
                         {
                             Name = a.Name,
                             Src = a.Src,
                             TodoId = a.TodoId,
                             UploadFileId = a.UploadFileId
                         }).SingleOrDefault();

            if (result == null)
            {
                return NotFound("找不到檔案");
            }
            return result;
        }

        // POST api/<TodoUploadFileController>
        [HttpPost]
        public string Post(Guid TodoId,[FromBody] UploadFileDto value)
        {
            if(!_todoContext.TodoLists.Any(a => a.TodoId == TodoId))
            {
                return "找不到該事項";
            }

            UploadFile insert = new UploadFile
            {
                Name = value.Name,
                Src = value.Src,
                TodoId = value.TodoId,
                UploadFileId = value.UploadFileId
            };

            _todoContext.UploadFiles.Add(insert);
            _todoContext.SaveChanges();

            return "OK";

        }

  

        // PUT api/<TodoUploadFileController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TodoUploadFileController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private static TodoListDto ItemToDto(TodoList a)
        {
          
            List<UploadFileDto> updto = new List<UploadFileDto>();

            foreach (var temp in a.UploadFiles)
            {
                UploadFileDto up = new UploadFileDto
                {
                    Name = temp.Name,
                    Src = temp.Src,
                    TodoId = temp.TodoId,
                    UploadFileId = temp.UploadFileId,
                };
                updto.Add(up);
            }

            return new TodoListDto
            {
                Enable = a.Enable,
                InsertEmployeeName = a.InsertEmployee.Name,
                InsertTime = a.InsertTime,
                Name = a.Name,
                Orders = a.Orders,
                TodoId = a.TodoId,
                UpdateEmployeeName = a.UpdateEmployee.Name,
                UpdateTime = a.UpdateTime,
                UploadFiles = updto,
            };
        }
    }
}
