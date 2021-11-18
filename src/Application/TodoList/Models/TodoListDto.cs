﻿namespace Exemplum.Application.TodoList.Models;

using Common.Mapping;
using Domain.Todo;

public class TodoListDto : IMapFrom<TodoList>
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Colour { get; set; } = string.Empty;
}