namespace MessagingApp.Models
{
    public class Message
    {
        public required string SenderId { get; set; }
        public required string ReceiverId { get; set; }
        public required string MessageText { get; set; }
        public required string DateTime { get; set; }
        public required bool Status { get; set; }

        public Message() { }

        public Message(string senderId, string receiverId, string message, string dateTime, bool status)
        {
            SenderId = senderId;
            ReceiverId = receiverId;
            MessageText = message;
            DateTime = dateTime;
            Status = status;
        }
    }
}