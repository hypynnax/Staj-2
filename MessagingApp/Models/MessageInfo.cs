namespace MessagingApp.Models
{
    public class MessageInfo
    {
        public required string PhotoUrl { get; set; }
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public required string Nickname { get; set; }
        public required string MessageText { get; set; }
        public required string DateTime { get; set; }
        public required bool Status { get; set; }

        public MessageInfo() { }

        public MessageInfo(string photoUrl, string senderId, string receiverId, string nickname, string message, string dateTime, bool status)
        {
            PhotoUrl = photoUrl;
            SenderId = senderId;
            ReceiverId = receiverId;
            Nickname = nickname;
            MessageText = message;
            DateTime = dateTime;
            Status = status;
        }
    }
}