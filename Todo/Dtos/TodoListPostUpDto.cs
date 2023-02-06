using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
//using Todo.Abstracts;
using Todo.Models;
using Todo.ValidationAttributes;

namespace Todo.Dtos
{
    public class TodoListPostUpDto
    {
        [ModelBinder(BinderType = typeof(FormDataJsonBinder))]
        public TodoListPostDto TodoList { get; set; }
        public IFormFileCollection files { get; set; }
    }
}
