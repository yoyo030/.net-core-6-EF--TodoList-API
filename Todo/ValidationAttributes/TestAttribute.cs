using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Todo.Dtos;
using Todo.Models;

namespace Todo.ValidationAttributes
{
    public class TestAttribute : ValidationAttribute
    {//請去看TodoListPostDto

        private string _tvalue;
        //private string _tvalue="123";也可以這樣寫,這樣Dto那邊就不用傳值
        public TestAttribute(string tvalue)
        {
            _tvalue = tvalue;
        }

        //也可以這樣寫,這樣Dto那邊就不用傳值
        //public TestAttribute(string tvalue= "123")
        //{
        //    _tvalue = tvalue;
        //}

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
           
            var st = (TodoListPostDto)value;

            return new ValidationResult(_tvalue, new string[] { "tvalue" });
        }
    }
}
