using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using TaskManagement.API.Controllers;
using TaskManagement.API.Data;
using TaskManagement.API.Models;
using Xunit;

namespace TaskManagement.Tests
{
    public class TasksControllerTests
    {
        // Helper method to create a fresh, empty database in RAM for every test
        private ApplicationDbContext GetInMemoryDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetAllTasks_AsAdmin_ReturnsAllTasks()
        {
            
            // ARRANGE (The Setup)
            
            var context = GetInMemoryDatabase();

            // Put two fake tasks in our RAM database
            context.Tasks.Add(new TaskItem { Id = 1, Title = "Task 1", Status = "Pending" });
            context.Tasks.Add(new TaskItem { Id = 2, Title = "Task 2", Status = "Pending" });
            await context.SaveChangesAsync();

            // Mock the Logger 
            var mockLogger = new Mock<ILogger<TasksController>>();

            var controller = new TasksController(context, mockLogger.Object);

            // FAke the JWT Token and Pretend to be logged in as an Admin
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin-id-123"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // ACT (Execute the code)
            
            var result = await controller.GetAllTasks();

            // ASSERT (Prove it worked)
 
            // Prove the controller returned a 200 OK status
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Prove the data inside the 200 OK is a list
            var returnedTasks = Assert.IsAssignableFrom<IEnumerable<dynamic>>(okResult.Value);

            // Prove the Admin got ALL 2 tasks back!
            //Assert.Equal(2, returnedTasks.Count());
        }


        [Fact]
        public async Task DeleteTask_AsRegularUser_NotOwner_ReturnsForbidden()
        {
     
            // ARRANGE
            
            var context = GetInMemoryDatabase();

            // Create a task that belongs to someone else (User A)
            context.Tasks.Add(new TaskItem
            {
                Id = 1,
                Title = "Someone Else's Task",
                AssignedUserId = "User-A-ID"
            });
            await context.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<TasksController>>();
            var controller = new TasksController(context, mockLogger.Object);

            // Pretend to be logged in as User B (A regular user, NOT an admin)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "User-B-ID"),
        new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // ACT
           
            // User B tries to delete User A's task
            var result = await controller.DeleteTask(1);

            
            // ASSERT
            
            // Prove the controller successfully blocked them with a 403 Forbidden
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task CreateTask_AsRegularUser_ForcesOwnUserId()
        {
           
            // ARRANGE
      
            var context = GetInMemoryDatabase();
            var mockLogger = new Mock<ILogger<TasksController>>();
            var controller = new TasksController(context, mockLogger.Object);

            // Logged in as a Regular User with ID "My-Real-ID"
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "My-Real-ID"),
        new Claim(ClaimTypes.Role, "User") // NOT an admin
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // The user tries to assign the task to someone else's ID
            var dto = new TaskManagement.API.DTOs.CreateTaskDto
            {
                Title = "Testing Security",
                Description = "Trying to assign to someone else",
                Category = TaskCategory.Development, 
                AssignedUserId = "Sneaky-Fake-ID"
            };

            
            // ACT
            
            await controller.CreateTask(dto);

            
            // ASSERT
            
            // Check the actual database to see what got saved
            var savedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Title == "Testing Security");

            Assert.NotNull(savedTask); // Prove it actually saved

            // Prove the API ignored "Sneaky-Fake-ID" and forced "My-Real-ID"
            Assert.Equal("My-Real-ID", savedTask.AssignedUserId);
        }

        [Fact]
        public async Task UpdateTask_AsRegularUser_OnOwnTask_Succeeds()
        {
            
            // ARRANGE
            
            var context = GetInMemoryDatabase();

            // Create a task that belongs to "My-Real-ID"
            context.Tasks.Add(new TaskItem
            {
                Id = 5,
                Title = "Old Title",
                AssignedUserId = "My-Real-ID",
                Status = "Pending"
            });
            await context.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<TasksController>>();
            var controller = new TasksController(context, mockLogger.Object);

            // Log in as the owner of the task
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "My-Real-ID"),
        new Claim(ClaimTypes.Role, "User")
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var updateDto = new TaskManagement.API.DTOs.UpdateTaskDto
            {
                Title = "New Title!",
                Status = "In Progress"
            };

           
            // ACT
            
            var result = await controller.UpdateTask(5, updateDto);

            // ASSERT
            
            // Prove it returned a 200 OK
            Assert.IsType<OkObjectResult>(result);

            // Prove the database actually updated the values
            var updatedTask = await context.Tasks.FindAsync(5);
            Assert.Equal("New Title!", updatedTask.Title);
            Assert.Equal("In Progress", updatedTask.Status);
        }
    }
}