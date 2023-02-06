using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Todo.Dtos;
using Todo.Models;

namespace Todo.Abstracts
{
    //這個類別不想被別人new出來 就把它寫成abstract
    public abstract class TodoListEditDtoAbstract : IValidatableObject
    {
        public string Name { get; set; }

        public bool Enable { get; set; }

        [Range(2, 3)]
        public int Orders { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<UploadFilePostDto> UploadFiles { get; set; }

        public TodoListEditDtoAbstract()
        {
            UploadFiles = new List<UploadFilePostDto>();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            TodoContext _todoContext = (TodoContext)validationContext.GetService(typeof(TodoContext));

            var findName = from a in _todoContext.TodoLists
                           where a.Name == Name
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
                yield return new ValidationResult("已存在相同的代辦事項", new string[] { "name" });
            }

            if (StartTime >= EndTime)
            {
                yield return new ValidationResult("開始時間不可以大於結束時間", new string[] { "time" });
            }

        }

    }
}
