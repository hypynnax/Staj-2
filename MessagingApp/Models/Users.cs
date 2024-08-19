namespace MessagingApp.Models
{
    public class Users
    {
        public required string PhotoUrl { get; set; }
        public required string Name { get; set; }
        public required string SurName { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public required string Password { get; set; }

        public Users() { }

        public Users(string photoUrl, string name, string surName, string email, string phoneNumber, string password)
        {
            PhotoUrl = photoUrl;
            Name = name;
            SurName = surName;
            Email = email;
            PhoneNumber = phoneNumber;
            Password = password;
        }
    }
}