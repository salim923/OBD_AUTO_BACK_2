using OBD.Domain.Entities;

public class MessageRecipient
{
    public int Id { get; set; }
    public int MessageId { get; set; }

    public int UserId { get; set; }

}