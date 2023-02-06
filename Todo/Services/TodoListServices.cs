using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Todo.Dtos;
using Todo.Models;
using Todo.Parameters;

namespace Todo.Services
{
    public class TodoListService
    {
        public string type => "1";



        private readonly TodoContext _todoContext;
        private readonly IMapper _mapper;//也可以在這邊使用AutoMapper這邊就點到為止不示範
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TodoListService(TodoContext todoContext, IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
        {
            _todoContext = todoContext;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
       
        public List<TodoListDto> GetDate([FromQuery] TodoSelectParameter? value)
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
            return result.ToList().Select(a => ItemToDto(a)).ToList();
        }

        public TodoListDto GetOne(Guid TodoId)
        {
            var result = (from a in _todoContext.TodoLists
                          where a.TodoId == TodoId
                          select a)
               .Include(a => a.UpdateEmployee)
               .Include(a => a.InsertEmployee)
               .Include(a => a.UploadFiles)
               .SingleOrDefault();
            if(result != null)
            {
                return ItemToDto(result);
            }


            return null;
        }

        public TodoList Post(TodoListPostDto value)
        {
            //cookie
            //var Claim = _httpContextAccessor.HttpContext.User.Claims.ToList();
            //var employeeid = Claim.Where(a => a.Type == "EmployeeId").First().Value;

            //jwt
            var employeeid = _httpContextAccessor.HttpContext.User.FindFirstValue("EmployeeId");
            var email = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);
            TodoList insert = new TodoList
            {               
                InsertTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                InsertEmployeeId = Guid.Parse(employeeid),
                UpdateEmployeeId = Guid.Parse(employeeid),
            };

            foreach (var temp in value.UploadFiles)
            {
                insert.UploadFiles.Add(new UploadFile()
                {
                    Name = temp.Name,
                    Src = temp.Src,
              
                });

                
            }
            _todoContext.TodoLists.Add(insert).CurrentValues.SetValues(value);
            _todoContext.SaveChanges();
            return insert;
        }

        public int Put(Guid id, [FromBody] TodoListPutDto value)
        {
            

            var update = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).SingleOrDefault();

            if (update != null)
            {               
                update.UpdateTime = DateTime.Now;                
                update.UpdateEmployeeId = Guid.Parse("00000000-0000-0000-0000-000000000001");

                _todoContext.TodoLists.Update(update).CurrentValues.SetValues(value);
            }


            return _todoContext.SaveChanges();
        }

    
        public int Delete(Guid id)
        {
            var delete = (from a in _todoContext.TodoLists
                          where a.TodoId == id
                          select a).Include(c => c.UploadFiles).SingleOrDefault();
            if (delete != null)
            {
                _todoContext.TodoLists.Remove(delete);
            }

            return _todoContext.SaveChanges();
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
