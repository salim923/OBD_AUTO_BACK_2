using System.Collections.Generic;

namespace OBD.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public DateTime DateSent { get; set; }
        public string Status { get; set; }
        public string Content { get; set; }

   
        public ICollection<MessageRecipient> MessageRecipients { get; set; } = new List<MessageRecipient>();
    }

    public class MessageInsert
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Content { get; set; }
        public List<string> RecipientEmails { get; set; } = new List<string>();
    }





}
