using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using Todo.Models;
using Todo.Dtos;
using Todo.Services;
using Todo.Parameters;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using AutoMapper;
using Todo.Abstracts;
using Azure;
using Microsoft.AspNetCore.JsonPatch;
using JsonPatchDocument = Microsoft.AspNetCore.JsonPatch.JsonPatchDocument;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]//這行會誘發物件資料驗證,WebMVC不會有這行,所以要在function內寫
    //if(!ModelState.IsValid)則..
    public class TodoController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly TodoContext _todoContext;
        private readonly IWebHostEnvironment _env;

        private readonly TodoListService _todoListService;//友用到服務才需要,Program.cs也要加入
        // GET: api/<TodoController>

        public TodoController(TodoContext todoContext, IMapper mapper, TodoListService todoListService, IWebHostEnvironment env)
        {
            _env = env;//注入根目錄
            _todoContext = todoContext;
            _mapper = mapper;
            _todoListService = todoListService;
            _env = env;
        }

        //模糊查詢 輸入關鍵字查詢
        [HttpGet]
        public IEnumerable<TodoListDto> Get([FromQuery] TodoSelectParameter? value)
        {
            var result = _todoContext.TodoLists
                .Include(a => a.UpdateEmployee)
                .Include(a => a.InsertEmployee)
                .Include(a => a.UploadFiles)
                .Select(a => a);

            if (!string.IsNullOrWhiteSpace(value.name))
            {
                result = result.Where(a => a.Name.IndexOf(value.name) > -1);
            }
            if (value.enable != null)
            {
                result = result.Where(a => a.Enable == value.enable);
            }
            if (value.InsertTime != null)
            {
                result = result.Where(a => a.InsertTime.Date == value.InsertTime);
            }

            if (value.minOrder != null && value.maxOrder != null)
            {
                result = result.Where(a => a.Orders >= value.minOrder && a.Orders <= value.maxOrder);
            }
            return result.ToList().Select(a => ItemToDto(a));
        }


        //模糊查詢 by Service(前面要寫這行private readonly TodoListService todoListService;)
        [HttpGet("GetByService")]
        //[Authorize]//如果需要驗證的話就寫這個標籤(目前Program.cs那邊有全部都家驗證了,這邊劉這個指示示範單一)
        [Authorize(Roles = "select")]//Role
        public IActionResult GetByService([FromQuery] TodoSelectParameter? value)
        {
            //Controller為控制邏輯 撈資料和盼端資料有沒有而已
            //控制邏輯與商業邏輯拆開
            var result = _todoListService.GetDate(value);
            if (result == null || result.Count() <= 0)
            {
                return NotFound("找不到資源");
            };
            return Ok(result);
        }

        //Id查詢
        // GET api/<TodoController>/5
        [HttpGet("{id}")]
        public TodoListDto Get(Guid id)
        {
            var result = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select new TodoListDto
                          {
                              Enable = a.Enable,
                              InsertEmployeeName = a.InsertEmployee.Name,
                              InsertTime = a.InsertTime,
                              Name = a.Name,
                              Orders = a.Orders,
                              TodoId = a.TodoId,
                              UpdateEmployeeName = a.UpdateEmployee.Name,
                              UpdateTime = a.UpdateTime,
                              UploadFiles = (from b in _todoContext.UploadFiles
                                             where a.TodoId == b.TodoId
                                             select new UploadFileDto
                                             {
                                                 Name = b.Name,
                                                 Src = b.Src,
                                                 TodoId = b.TodoId,
                                                 UploadFileId = b.UploadFileId
                                             }).ToList()
                          }).SingleOrDefault();
            return result;
        }

        //Id查詢
        // GET api/<TodoController>/5
        [HttpGet("GetOneByService/{id}")]
        public ActionResult<TodoListDto> GetOneByService(Guid id)
        {
            var result = _todoListService.GetOne(id);
            if (result == null)
            {
                return NotFound("找不到資源");
            };
            return result;
        }

        //用SQL撈資料
        [HttpGet("GetSoLDto")]
        public IEnumerable<TodoListDto> GetSOLDto(string name)
        {
            string sql = @"SELECT [Todold]
                           ,a.[Namc]
                           ,[InsertTime]
                           .IUpdateTime|
                           ,lEnablel
                           ,[Ordcrs]
                           ,b.NancasInsertEnployecName
                           ,c.NameasUpdateEmployeeName
                           [RCM[TodoList]a
                           joinEmploveebona.Insertimployeeld-b.Eimployeeld
                           join Employee c on a.UpdateEmployeeId=c.Employeeld where 1=1";

            if (!string.IsNullOrWhiteSpace(name)) {
                sql = sql + "and name like N'%" + name + "%'";
            }

            var result = _todoContext.ExecSQL<TodoListDto>(sql);

            return result;
        }


        // POST api/<TodoController>
        [HttpPost]
        public IActionResult Post([FromBody] TodoListPostDto value)
        {
            List<UploadFile> upl = new List<UploadFile>();

            foreach (var temp in value.UploadFiles)
            {
                UploadFile up = new UploadFile()
                {
                    Name = temp.Name,
                    Src = temp.Src,
                };
                upl.Add(up);
            }

            TodoList insert = new TodoList
            {
                Name = value.Name,
                Enable = value.Enable,
                Orders = value.Orders,
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UploadFiles = upl//有外鍵才可這樣做,直接存不用下面那一大段
            };

            //_todoContext.Add(value);兩種寫法都可以
            _todoContext.TodoLists.Add(insert);
            _todoContext.SaveChanges();

            //foreach (var temp in value.UploadFiles)
            //{
            //    UploadFile insert2 = new UploadFile
            //    {
            //        Name = temp.Name,
            //        Src = temp.Src,
            //        TodoId = insert.TodoId
            //    };

            //    _todoContext.UploadFiles.Add(insert2);
            //}

            //_todoContext.SaveChanges();

            return CreatedAtAction(nameof(Get), new { id = insert.TodoId }, insert);
        }

        // POST api/<TodoController>
        [HttpPost("easy")]
        public void PostEasy([FromBody] TodoListPostDto value)
        {
            //只填系統要自己帶的值,使用者填的值不用帶,紙袋使用者沒有填但系統要自己填的值
            TodoList insert = new TodoList
            {
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            };

            _todoContext.TodoLists.Add(insert).CurrentValues.SetValues(value);
            //CurrentValues後面的程式碼會自動幫忙匹配
            _todoContext.SaveChanges();//先存檔是因為要先拿到TodoId


            //這裡是因為他不會自動幫忙匹配UploadFiles,因為兩個名稱雖然一樣但類別不一樣,一個是UploadFile,一個是UploadFilePostDto
            foreach (var temp in value.UploadFiles)
            {
                _todoContext.UploadFiles.Add(new UploadFile()
                {
                    TodoId = insert.TodoId//TodoId
                }).CurrentValues.SetValues(temp);
            }

            _todoContext.SaveChanges();
        }

        //無外鍵版本
        // POST api/<TodoController>
        [HttpPost("nofk")]
        public void Postnofk([FromBody] TodoListPostDto value)
        {
            TodoList insert = new TodoList
            {
                Name = value.Name,
                Enable = value.Enable,
                Orders = value.Orders,
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                //UploadFiles = upl//有外鍵才可這樣做,直接存不用下面那一大段
            };

            //_todoContext.Add(value);兩種寫法都可以
            _todoContext.TodoLists.Add(insert);
            _todoContext.SaveChanges();

            foreach (var temp in value.UploadFiles)
            {
                UploadFile insert2 = new UploadFile
                {
                    Name = temp.Name,
                    Src = temp.Src,
                    TodoId = insert.TodoId
                };

                _todoContext.UploadFiles.Add(insert2);
            }

            _todoContext.SaveChanges();


        }


        //SQL版本
        // POST api/<TodoController>
        [HttpPost("sql")]
        public void Postsql([FromBody] TodoListPostDto value)
        {
            var name = new SqlParameter("name", value.Name);
            var insertTime = new SqlParameter("name", DateTime.Now);
            var updateTime = new SqlParameter("name", DateTime.Now);
            var orders = new SqlParameter("name", value.Orders);
            var insertEmployeeId = new SqlParameter("name", Guid.Parse("00000000-0000-0000-0000-000000000001"));
            var updateEmployeeId = new SqlParameter("name", Guid.Parse("00000000-0000-0000-0000-000000000001"));
            string sql = @"INSERT INTO [dbo].[TodoList]
                           ([Name]
                          ,[InsertTime]
                          ,[UpdateTime]
                          ,[Enable]
                          ,[Orders]
                          ,[InsertEmployeeId]
                          ,[UpdateEmployeeId])
                           (@name,@insertTime,@updateTime,1,@Orders,@InsertEmployeeId,@UpdateEmployeeId)";

            _todoContext.Database.ExecuteSqlRaw(sql, name, insertTime, updateTime, orders, insertEmployeeId, updateEmployeeId);


        }


        // POST api/<TodoController>
        [HttpPost("PostByService")]
        public IActionResult PostByService([FromBody] TodoListPostDto value)
        {
            var insert = _todoListService.Post(value);

            return CreatedAtAction(nameof(Get), new { id = insert.TodoId }, insert);
        }

        // PUT api/<TodoController>/5
        [HttpPut("{id}")]
        public void Put(Guid id, [FromBody] TodoList value)
        {
            _todoContext.TodoLists.Update(value);
            //_todoContext.Entry(value).State = EntityState.Modified;兩種方法都可
            _todoContext.SaveChanges();

        }

        // PUT api/<TodoController>/5
        [HttpPut("PutCustomer/{id}")]//這個比較正確因為有使用Dto
        public IActionResult PutCustomer(Guid id, [FromBody] TodoListPutDto value)
        {
            if (id != value.TodoId)
            {
                //要跳錯誤
                return BadRequest();
            }

            //_todoContext.TodoLists.Update(value);           
            //_todoContext.SaveChanges();

            //var update = _todoContext.TodoLists.Find(id);

            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();

            if (update != null)
            {
                update.InsertTime = DateTime.Now;
                update.UpdateTime = DateTime.Now;
                update.InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                update.Name = value.Name;
                update.Enable = value.Enable;
                update.Orders = value.Orders;

                _todoContext.SaveChanges();
            }
            else
            {
                return NotFound();
            }

            return NoContent();
        }

        // PUT api/<TodoController>/5
        [HttpPut("PutNoIdNoRestFul")]//無帶Id但不符合restful,不過也是可以只是不符合restful
        public void PutCustomer( [FromBody] TodoListPutDto value)
        {
            //_todoContext.TodoLists.Update(value);           
            //_todoContext.SaveChanges();

            //var update = _todoContext.TodoLists.Find(id);

            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == value.TodoId
                          select a).SingleOrDefault();

            if (update != null)
            {
                update.InsertTime = DateTime.Now;
                update.UpdateTime = DateTime.Now;
                update.InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                update.Name = value.Name;
                update.Enable = value.Enable;
                update.Orders = value.Orders;

                _todoContext.SaveChanges();
            }
        }

        [HttpPut("AutoMapper/{id}")]
        public void PutAutoMapper(Guid id, [FromBody] TodoListPutDto value)
        {

            //var update = _todoContext.TodoLists.Find(id);

            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();

            if (update != null)
            {

                _mapper.Map(value, update);
                _todoContext.SaveChanges();
            }
        }

        // PUT api/<TodoController>/5
        [HttpPut("PutFast")]
        public void PutFast([FromBody] TodoListPutDto value)
        {
            //_todoContext.TodoLists.Update(value);           
            //_todoContext.SaveChanges();

            //var update = _todoContext.TodoLists.Find(id);

            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == value.TodoId
                          select a).SingleOrDefault();

            if (update != null)
            {
                update.InsertTime = DateTime.Now;
                update.UpdateTime = DateTime.Now;
                update.InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                _todoContext.TodoLists.Add(update).CurrentValues.SetValues(value);
                _todoContext.SaveChanges();
            }
        }

        // PUT api/<TodoController>/5
        [HttpPut("PutByService/{id}")]//這個比較正確因為有使用Dto
        public IActionResult PutByService(Guid id, [FromBody] TodoListPutDto value)
        {
            if (id != value.TodoId)
            {
                //要跳錯誤
                return BadRequest();
            }

            
            if(_todoListService.Put(id,value) == 0)
            {
                return NotFound();
            }

            return NoContent();
        }


        // Patch要多安裝兩個套件JsonPath 和MVC JSON
        [HttpPatch("{id}")]//這個比較正確因為有使用Dto
        public void Patch(Guid id, [FromBody] JsonPatchDocument value)
        {
            //要送入
            //[{
            //    "op" : "replace" ,
            //    "path" : "/name" ,
            //    "value":"YOYOY"
            //}]

            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();

            if (update != null)
            {
                update.UpdateTime = DateTime.Now;              
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                value.ApplyTo(update);


                _todoContext.SaveChanges();
            }
        }


        // DELETE api/<TodoController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var delete = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).Include(c=>c.UploadFiles).SingleOrDefault();
            if (delete == null)
            {
                return NotFound("找不到");
            }
          

            _todoContext.TodoLists.Remove(delete);
            _todoContext.SaveChanges();

            return NoContent();
        }

        // DELETE api/<TodoController>/5
        [HttpDelete("nofk/{id}")]
        public void NofkDelete(Guid id)
        {
            var child = from a in _todoContext.UploadFiles
                        where a.TodoId == id
                        select a;
            _todoContext.UploadFiles.RemoveRange(child);//整批刪除


            var delete = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();
            if (delete != null)
            {
                _todoContext.TodoLists.Remove(delete);
                _todoContext.SaveChanges();
            }
        }
        
        // DELETE api/<TodoController>/5
        [HttpDelete("list/{ids}")]
        public void Delete(string ids)
        {
            //送入網址https://localhost:44360/api/todo/list/["6fc0b116-932f-4fdf-a180-3e8ac6a05351" ,"2978e7fe-3803-469e-ad44-4e222addf08b"]
            var deleteList = JsonSerializer.Deserialize<List<Guid>>(ids);


            var delete = (from a in _todoContext.TodoLists
                          where deleteList.Contains(a.TodoId)
                          select a).Include(c => c.UploadFiles);
            if (delete != null)
            {
                _todoContext.TodoLists.RemoveRange(delete);
                _todoContext.SaveChanges();
            }
        }

        // DELETE api/<TodoController>/5
        [HttpDelete("DeleteByService/{id}")]
        public IActionResult DeleteByService(Guid id)
        {
            if (_todoListService.Delete(id) == 0)
            {
                return NotFound("找不到");
            }
            return NoContent();
        }

       
        [HttpPost("up")]
        //要看這裡一下 這裡沒使用Binder
        //public void PostUp(/*[FromForm] TodoListPostDto value*/[FromForm] string value)
        //{
        //    //[FromForm] TodoListPostDto value 
        //    //Form來的資料是字串 要自己轉成物件
        //    TodoListPostDto aa = JsonSerializer.Deserialize<TodoListPostDto>(value);

        //}
        public void PostUp([FromForm] TodoListPostUpDto value)
        {
            TodoList insert = new TodoList
            {
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                InsertEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001")
            };

            _todoContext.TodoLists.Add(insert).CurrentValues.SetValues(value.TodoList);
            _todoContext.SaveChanges();


            string rootRoot = _env.ContentRootPath + @"\wwwroot\UploadFiles\" + insert.TodoId + "\\";

            if (!Directory.Exists(rootRoot))
            {
                Directory.CreateDirectory(rootRoot);
            }

            foreach (var file in value.files)
            {
                string fileName = file.FileName;

                using (var stream = System.IO.File.Create(rootRoot + fileName))
                {
                    file.CopyTo(stream);

                    var insert2 = new UploadFile
                    {
                        Name = fileName,
                        Src = "/UploadFiles/" + insert.TodoId + "/" + fileName,
                        TodoId = insert.TodoId
                    };

                    _todoContext.UploadFiles.Add(insert2);
                }
            }

            _todoContext.SaveChanges();
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
