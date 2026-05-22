using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using System.Security.Claims;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Controllers
{
    [Authorize] // SECURITY: This locks down the entire controller. No JWT = No Access.
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TasksController> _logger;

        // Dependency Injection: database context
        public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            // Extract the ID of whoever just called the API
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Scenario A: The user is an Admin
            if (User.IsInRole("Admin"))
            {
                // Fetch all tasks
                var allTasks = await _context.Tasks
                    .Include(t => t.AssignedUser) // Grab the linked user row
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.DueDate,
                        t.Status,
                        t.Priority,
                        t.Category,
                        // If there's a user, send their email. Otherwise, say "Unassigned"
                        AssignedTo = t.AssignedUser != null ? t.AssignedUser.Email : "Unassigned"
                    })
                    .ToListAsync();

                return Ok(allTasks);
            }

            // Scenario B: A regular user
            // Only fetch tasks where the ID matches their token
            var myTasks = await _context.Tasks
                .Where(t => t.AssignedUserId == currentUserId)
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    t.Status,
                    t.Priority,
                    t.Category,
                    AssignedTo = "Me" //  only see their own tasks
                })
                .ToListAsync();

            return Ok(myTasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Find the task and include the User data
            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            // If it doesn't exist, return 404
            if (task == null) return NotFound(new { message = "Task not found." });

            //If it's a regular user, check if they own this task
            if (!isAdmin && task.AssignedUserId != currentUserId)
            {
                return Forbid(); // 403 Forbidden:  cannot view someone else's task
            }

            
            var result = new
            {
                task.Id,
                task.Title,
                task.Description,
                task.DueDate,
                task.Status,
                task.Priority,
                task.Category,
                AssignedTo = task.AssignedUser != null ? task.AssignedUser.Email : "Unassigned"
            };

            return Ok(result);
        }

        // POST: api/tasks
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only admin can create task
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            // Get the ID of the person making the request
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Map the DTO to our actual Database Model
            var newTask = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                Category = dto.Category,
                Status = "Pending", // Every new task starts as Pending

                // If the frontend didn't send an AssignedUserId, assign it to the logged-in user
                AssignedUserId = string.IsNullOrEmpty(dto.AssignedUserId) ? currentUserId : dto.AssignedUserId
            };

            // Add to database and save
            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            // Record the event in the server logs
            _logger.LogInformation("A new task titled '{TaskTitle}' was created by User ID: {UserId}", newTask.Title, currentUserId);

            return Ok(new { message = "Task created successfully!", task = newTask });
        }



        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only admin can delete a task
        public async Task<IActionResult> DeleteTask(int id)
        {
            // Find a task with this specific ID
            var task = await _context.Tasks.FindAsync(id);

            // If the task doesn't exist, return a 404 error
            if (task == null)
            {
                return NotFound(new { message = "Task not found." });
            }

            // task for deletion
            _context.Tasks.Remove(task);

            // execute the deletion in SQL Server
            await _context.SaveChangesAsync();

            //Return a success message to the frontend
            return Ok(new { message = $"Task {id} was permanently deleted." });
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
        {
            // Check the user and their role
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Find the task
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound(new { message = "Task not found." });

            // If it's a regular user, check if they  own this task
            if (!isAdmin && task.AssignedUserId != currentUserId)
            {
                return Forbid(); // 403 Forbidden: You cannot edit someone else's task
            }

            // Apply changes according to role

            // Admins are allowed to update ANY field
            if (isAdmin)
            {
                if (dto.Title != null) task.Title = dto.Title;
                if (dto.Description != null) task.Description = dto.Description;
                if (dto.DueDate != null) task.DueDate = dto.DueDate;
                if (dto.Priority != null) task.Priority = dto.Priority;
                if (dto.Category != null) task.Category = dto.Category;
                if (dto.AssignedUserId != null) task.AssignedUserId = dto.AssignedUserId;
            }

            // both Admins and Regular Users are allowed to update the Status
            if (dto.Status != null) task.Status = dto.Status;

            // Save changes to SQL Server
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task updated successfully!", task });
        }
    }
}