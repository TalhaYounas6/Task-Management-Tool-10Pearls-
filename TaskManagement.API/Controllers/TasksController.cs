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
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Priority = dto.Priority,
                Category = dto.Category,
                CreatorUserId = currentUserId,
                // If Admin use the DTO. If User then their own ID 
                AssignedUserId = isAdmin ? dto.AssignedUserId : currentUserId,
                Status = "Pending"
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("A new task titled '{TaskTitle}' was created.", task.Title);

            return Ok(new { message = "Task created successfully!", task });
        }



        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        // Removed [Authorize(Roles = "Admin")] so regular users can trigger this method
        public async Task<IActionResult> DeleteTask(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound(new { message = "Task not found." });

            // Block if they are not an Admin and they don't own the task
            if (!isAdmin && task.AssignedUserId != currentUserId)
            {
                return Forbid();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

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

            // If it's a regular user, check if they own this task
            if (!isAdmin && task.AssignedUserId != currentUserId)
            {
                return Forbid(); // 403 Forbidden: You cannot edit someone else's task
            }

            // Apply Changes

            // Only Admins can reassign a task to someone else
            if (isAdmin && dto.AssignedUserId != null)
            {
                task.AssignedUserId = dto.AssignedUserId;
            }

            // Because of the Forbid() check above, if a regular user makes it 
            // to this line, it is guaranteed to be their task. So they can update these details
            if (dto.Title != null) task.Title = dto.Title;
            if (dto.Description != null) task.Description = dto.Description;
            if (dto.DueDate != null) task.DueDate = dto.DueDate;
            if (dto.Status != null) task.Status = dto.Status;
            if (dto.Priority != null) task.Priority = dto.Priority;
            if (dto.Category != null) task.Category = dto.Category; 

            // Save changes to SQL Server
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task updated successfully!", task });
        }


    }
}