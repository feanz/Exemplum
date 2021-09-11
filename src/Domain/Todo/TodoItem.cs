﻿namespace Domain.Todo
{
    using Common;
    using Events;
    using System;

    public class TodoItem : BaseEntity
    {
        public TodoItem(string title)
        {
            Title = title;
        }

        public int ListId { get; private set; }
        public TodoList List { get; set; } = null!;

        public string Title { get; private set; } = string.Empty;

        public string Note { get; set; } = string.Empty;

        public PriorityLevel? Priority { get; private set; }

        public DateTime? Reminder { get; private set; }

        private bool _done;
        
        // Example of a property that can be set on init and also via behaviour on the entity
        public bool Done { get => _done; init => _done = value; }

        public void MarkAsDone()
        {
            _done = true;
            DomainEvents.Add(new TodoItemCompletedEvent(this));
        }
        
        // todo implement as a good example of business logic in smart enums
        // public void SetPriority(PriorityLevel priority)
        // {
        //     
        // }
    }
}