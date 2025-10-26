namespace Tagger.Models
{
    public class Staff
    {
        public Guid Id { get; set; }
        public long ChatId { get; set; }
        public string FirstName { get; set; }= String.Empty;
        public string LastName { get; set; }= String.Empty;
        public string LegacyName {get; set;}= String.Empty;
        
    }
}