namespace MessagingApp.Models
{
    public class FriendList
    {
        public required string UserId { get; set; }
        public required string Nickname { get; set; }

        public FriendList() { }

        public FriendList(string userId, string nickName)
        {
            UserId = userId;
            Nickname = nickName;
        }
    }
}