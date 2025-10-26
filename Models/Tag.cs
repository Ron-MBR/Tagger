namespace Tagger.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public long UserChatId { get; set; }
        public long StaffChatId { get; set; }

        public DateTime TagTime { get; set; }

    }
}