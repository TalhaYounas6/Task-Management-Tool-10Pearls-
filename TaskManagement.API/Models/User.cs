using Microsoft.AspNetCore.Identity;

namespace TaskManagement.API.Models
{
    // IdentityUser already has Email, UserName and other stuff
    public class User : IdentityUser
    {
        // Add custom fields here if needed (e.g., FirstName, LastName)
        public string FullName { get; set; } = string.Empty;
    }
}