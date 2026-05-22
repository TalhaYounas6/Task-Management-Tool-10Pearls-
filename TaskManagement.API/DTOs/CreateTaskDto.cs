using System.ComponentModel.DataAnnotations;

namespace TaskManagement.API.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public string Priority { get; set; } = "Medium";

        public string? Category { get; set; }

        // Optional: An admin can assign this to someone else. 
        // If left blank, it will just assign the task to the person creating it.
        public string? AssignedUserId { get; set; }
    }
}