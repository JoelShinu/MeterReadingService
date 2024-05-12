namespace api.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public string FirstName { get; set; } = string.Empty; //init as empty string
        public string LastName { get; set; } = string.Empty; //init as empty string
    }
}