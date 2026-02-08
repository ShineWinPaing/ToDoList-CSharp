using System.ComponentModel.DataAnnotations;

namespace ToDoList.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;

        public string? OTP { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}
