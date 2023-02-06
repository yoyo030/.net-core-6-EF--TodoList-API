using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Todo.Models;
using Todo.ValidationAttributes;
using Todo.Abstracts;

namespace Todo.Dtos;




public class TodoListPostDto: TodoListEditDtoAbstract
{
    
}

#region 2
//[StartEnd]//共用才寫成標籤,不是共用的可以直接寫在類別裡如下面註解的程式碼,下面有驗證name和time
//[Test("123")]
//public class TodoListPostDto
//{

//    //[Required(AllowEmptyStrings = false,ErrorMessage ="Name不可為空")]
//    //[Requircd]
//    //[EmailAddress(ErrorMessage = "請輸入電子信箱")]
//    //[StringLength(30)]
//    //[RegularExpression("[a-z]")]//正規表示式
//    [TodoName]
//    public string Name { get; set; }

//    public bool Enable { get; set; }

//    [Range(2, 3)]
//    public int Orders { get; set; }

//    public DateTime StartTime { get; set; }
//    public DateTime EndTime { get; set; }
//    public List<UploadFilePostDto> UploadFiles { get; set; }

//    public TodoListPostDto()
//    {
//        UploadFiles = new List<UploadFilePostDto>();
//    }

//}

#endregion

#region 3
//public class TodoListPostDto:IValidatableObject
//{
//    public string Name { get; set; }

//    public bool Enable { get; set; }

//    [Range(2, 3)]
//    public int Orders { get; set; }

//    public DateTime StartTime { get; set; }
//    public DateTime EndTime { get; set; }
//    public List<UploadFilePostDto> UploadFiles { get; set; }

//    public TodoListPostDto()
//    {
//        UploadFiles = new List<UploadFilePostDto>();
//    }

//    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
//    {
//        TodoContext _todoContext = (TodoContext)validationContext.GetService(typeof(TodoContext));

//        var findName = from a in _todoContext.TodoLists
//                       where a.Name == Name
//                       select a;

//        //更新的時候,避免抓到自己,然後說已經有存在的Name了 所有要多血衣些判斷
//        var dto = validationContext.ObjectInstance;//抓整個類別的其他所有欄位  

//        if (dto.GetType() == typeof(TodoListPutDto))//如果傳進來的類別是這個putDto
//        {
//            var updateDto = (TodoListPutDto)dto;
//            findName = findName.Where(a => a.TodoId != updateDto.TodoId);
//            //取不等於自己的Id的資料
//            //也就是排除掉自己的資料
//            //線面就不會被比對到
//        }

//        if (findName.FirstOrDefault() != null)
//        {
//            yield return new ValidationResult("已存在相同的代辦事項", new string[] { "name" });
//        }

//        if (StartTime >= EndTime)
//        {
//            yield return new ValidationResult("開始時間不可以大於結束時間", new string[] { "time" });
//        }

//    }

//}
#endregion