using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Todo.Models;
using Todo.ValidationAttributes;
using Todo.Abstracts;

namespace Todo.Dtos
{
    public class LoginPost
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }
}




