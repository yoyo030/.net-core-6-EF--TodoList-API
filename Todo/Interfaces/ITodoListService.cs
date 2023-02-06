using Microsoft.AspNetCore.Mvc;
using Todo.Dtos;
using Todo.Parameters;

namespace Todo.Interfaces
{
    public interface ITodoListService
    {
        string type { get; }
        List<TodoListDto> GetDate(TodoSelectParameter value);

    }
}
