using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagement.API.Data;
using TaskManagement.API.DTOs;
using TaskManagement.API.Models;

namespace TaskManagement.API.Controllers
{
    [Authorize] 
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<IActionResult> GetAllTasks()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Scenario A: The user is an Admin
            if (User.IsInRole("Admin"))
            {
                var allTasks = await _context.Tasks
                    .Include(t => t.AssignedUser)
                    .Select(t => new
                    {
                        t.Id,
                        t.Title,
                        t.Description,
                        t.DueDate,
                        t.Status,
                        t.Priority,
                        t.Category,
                        CreatorUserId = t.CreatorUserId,
                        AssignedUserId = t.AssignedUserId,
                        AssignedUserName = t.AssignedUser != null ? t.AssignedUser.FullName : null
                    })
                    .ToListAsync();

                return Ok(allTasks);
            }

            // Scenario B: A regular user
            var myTasks = await _context.Tasks
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == currentUserId || t.CreatorUserId == currentUserId) 
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Description,
                    t.DueDate,
                    t.Status,
                    t.Priority,
                    t.Category,
                    CreatorUserId = t.CreatorUserId, 
                    AssignedUserId = t.AssignedUserId,
                    AssignedUserName = t.AssignedUser != null ? t.AssignedUser.FullName : null
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

            var task = await _context.Tasks
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null) return NotFound(new { message = "Task not found." });

            // Ensure they are Admin, the Creator, or the Assigned User
            if (!isAdmin && task.AssignedUserId != currentUserId && task.CreatorUserId != currentUserId)
            {
                return Forbid();
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
                CreatorUserId = task.CreatorUserId, 
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = task.AssignedUser != null ? task.AssignedUser.FullName : null
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
        public async Task<IActionResult> DeleteTask(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound(new { message = "Task not found." });

            bool isCreator = task.CreatorUserId == currentUserId;

            // Block if they are not an Admin AND they didn't create the task. 
            // (Assigned users cannot delete tasks)
            if (!isAdmin && !isCreator)
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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound(new { message = "Task not found." });

            bool isCreator = task.CreatorUserId == currentUserId;
            bool isAssigned = task.AssignedUserId == currentUserId;

          
            if (!isAdmin && !isCreator && !isAssigned)
            {
                return Forbid();
            }

           
            if (dto.Status != null) task.Status = dto.Status;

         
            if (isAdmin || isCreator)
            {
                if (dto.Title != null) task.Title = dto.Title;
                if (dto.Description != null) task.Description = dto.Description;
                if (dto.DueDate != null) task.DueDate = dto.DueDate;
                if (dto.Priority != null) task.Priority = dto.Priority;
                if (dto.Category != null) task.Category = dto.Category;
            }

            
            if (isAdmin && dto.AssignedUserId != null)
            {
                task.AssignedUserId = dto.AssignedUserId;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Task updated successfully!", task });
        }
    }
}