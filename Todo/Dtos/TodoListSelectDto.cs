﻿using System;
using System.Collections.Generic;

namespace Todo.Dtos;

public  class TodoListDto
{
    public Guid TodoId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime InsertTime { get; set; }

    public DateTime UpdateTime { get; set; }

    public bool Enable { get; set; }

    public int Orders { get; set; }

    public string InsertEmployeeName { get; set; }

    public string UpdateEmployeeName { get; set; }

    public ICollection<UploadFileDto> UploadFiles{ get; set; }

}
