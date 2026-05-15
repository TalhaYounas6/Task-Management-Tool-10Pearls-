using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.API.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        
        [Required]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed

        [Required]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public string? Category { get; set; }

        // FOREIGN KEY: This will link the task to a specific User's ID
        public string? AssignedUserId { get; set; }

       
        [ForeignKey("AssignedUserId")]
        public User? AssignedUser { get; set; }
    }
}