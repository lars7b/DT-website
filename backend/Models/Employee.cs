namespace Backend.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Phone { get; set; }
        public string? Position { get; set; }
    }
}