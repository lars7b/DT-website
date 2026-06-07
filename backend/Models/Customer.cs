namespace Backend.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public required int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
