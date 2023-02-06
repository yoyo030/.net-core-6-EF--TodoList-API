using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Todo.Dtos;
using Todo.Interfaces;
using Todo.Parameters;
using Todo.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoIOCController : ControllerBase
    {
        //只有一個注入時這樣寫(1)
        //private readonly ITodoListService _todoListService;

        //多個注入時這樣寫(2)
        private readonly IEnumerable<ITodoListService> _todoListService;

        //只有一個注入時這樣寫(1)
        //public TodoIOCController(ITodoListService todoListService)
        //{
        //    _todoListService = todoListService;
        //}

        //多個注入時這樣寫(2)
        public TodoIOCController(IEnumerable<ITodoListService> todoListService)
        {
            _todoListService = todoListService;
        }
        [HttpGet]

        //只有一個注入時這樣寫(1)
        //public List<TodoListDto> Get([FromBody]TodoSelectParameter value)
        //{
        //    return _todoListService.GetDate(value);
        //}

        //多個注入時這樣寫(2)
        public List<TodoListDto> Get([FromBody] TodoSelectParameter value)
        {
            ITodoListService _todo;

            if (value.name.Length > 3)
            {
                _todo = _todoListService.Where(a => a.type == "1").Single();
            }
            else
            {
                _todo = _todoListService.Where(a => a.type == "2").Single();
            }
            return _todo.GetDate(value);
        }

    }
}
