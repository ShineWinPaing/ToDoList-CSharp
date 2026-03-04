using System.ComponentModel.DataAnnotations;

namespace ToDoList.Models
{
    public class UserModel
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string? FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? LastName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Bio { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public string FullName => string.IsNullOrWhiteSpace($"{FirstName} {LastName}")
                                  ? Username
                                  : $"{FirstName} {LastName}".Trim();

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? OTP { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}