using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Todo.Dtos;
using Todo.Models;

namespace Todo.ValidationAttributes
{
    public class TodoNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //第一次載類別抓資料庫資料所以要宣告
            //Controller是在建構子友宣告 且載Program.cs友送入服務 所以Controller function內不用再宣告
            TodoContext _todoContext = (TodoContext)validationContext.GetService(typeof(TodoContext));
           
            //新增Todo時要去查Name有沒有重複//請去看TodoListPostDto
            var name = (string)value;

            var findName = from a in _todoContext.TodoLists
                           where a.Name == name
                           select a;

            //更新的時候,避免抓到自己,然後說已經有存在的Name了 所有要多血衣些判斷
            var dto = validationContext.ObjectInstance;//抓整個類別的其他所有欄位  

            if (dto.GetType() == typeof(TodoListPutDto))//如果傳進來的類別是這個putDto
            {
                var updateDto = (TodoListPutDto)dto;
                findName = findName.Where(a => a.TodoId != updateDto.TodoId);
                //取不等於自己的Id的資料
                //也就是排除掉自己的資料
                //線面就不會被比對到
            }

            if (findName.FirstOrDefault() != null)
            {
                return new ValidationResult("已存在相同的代辦事項");
            }

            return ValidationResult.Success;
        }
    }
}
