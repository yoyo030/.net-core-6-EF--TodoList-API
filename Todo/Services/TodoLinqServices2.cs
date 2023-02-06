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
    //跟TodoLinqService一樣只是要練習同一個介面下不同實作
    public class TodoLinqService2:ITodoListService
    {
        public string type => "2";
        private readonly TodoContext _todoContext;
        private readonly IMapper _mapper;//也可以在這邊使用AutoMapper這邊就點到為止不示範

        public TodoLinqService2(TodoContext todoContext, IMapper mapper)
        {
            _todoContext = todoContext;
            _mapper = mapper;
        }

        //實作介面
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
