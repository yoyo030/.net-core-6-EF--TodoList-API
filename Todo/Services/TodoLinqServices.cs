using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Todo.Dtos;
using Todo.Interfaces;
using Todo.Models;
using Todo.Parameters;

namespace Todo.Services
{
    public class TodoLinqService:ITodoListService
    {
        private readonly TodoContext _todoContext;
        private readonly IMapper _mapper;//也可以在這邊使用AutoMapper這邊就點到為止不示範

        public string type => "1";

        public TodoLinqService(TodoContext todoContext, IMapper mapper)
        {
            _todoContext = todoContext;
            _mapper = mapper;
        }

        //實作介面
        public List<TodoListDto> GetDate([FromQuery] TodoSelectParameter? value)
        {
            //這段也可以在寫一個Repository的Class去做純撈資料的動作(架構問題也不一定要這樣做Repository為取資料邏輯,Services為商業邏輯)
            var result = _todoContext.TodoLists
               .Include(a => a.UpdateEmployee)
               .Include(a => a.InsertEmployee)
               .Include(a => a.UploadFiles)
               .Select(a => a);
            //如果這邊寫成需要呼叫Repository那個Class,記得前面要先注入
            //Repository上層也是要做一個IRepository

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
