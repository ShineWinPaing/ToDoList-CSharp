namespace ToDoList.Models
{
    public class ListItemModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string? Description { get; set; } = string.Empty;
        public int UserId { get; set; }
        public UserModel? User { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Now;
    }
}
