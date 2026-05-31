using System;
using TaskManagement.API.Models;

namespace TaskManagement.API.DTOs
{
    public class UpdateTaskDto
    {
        
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public TaskCategory? Category { get; set; }
        public string? AssignedUserId { get; set; }
    }
}