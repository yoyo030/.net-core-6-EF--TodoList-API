using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Todo.Dtos;
using Todo.Models;

namespace Todo.ValidationAttributes
{
    public class StartEndAttribute : ValidationAttribute
    {//請去看TodoListPostDto
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //這裡的value會傳回整個class,因為驗證寫在整個class最上面
            var st = (TodoListPostDto)value;

            if (st.StartTime >= st.EndTime) {
                return new ValidationResult("開始時間不可以大於結束時間", new string[] { "time"});
            }
            
            return ValidationResult.Success;
        }
    }
}
