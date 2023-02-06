using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Todo.Abstracts;

namespace Todo.Dtos;
public class TodoListPutDto : TodoListEditDtoAbstract
{
    public Guid TodoId { get; set; }
}

//public  class TodoListPutDto
//{
//    [Required]
//    public Guid TodoId { get; set; }
//    [Required(AllowEmptyStrings = false,ErrorMessage ="Name不可為空")]
//    //[Required]
//    [EmailAddress(ErrorMessage = "請輸入電子信箱")]
//    [StringLength(30)]
//    //[RegularExpression("[a-z]")]//正規表示式
//    public string Name { get; set; }

//    public bool Enable { get; set; }

//    [Range(2,3)]
//    public int Orders { get; set; }

//    public List<UploadFilePostDto> UploadFiles { get; set; }

//    public TodoListPutDto()
//    {
//        UploadFiles = new List<UploadFilePostDto>();  
//    }

//}
