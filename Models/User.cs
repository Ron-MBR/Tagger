using System.Dynamic;

namespace Tagger.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public long ChatId { get; set; }

        public string Name { get; set; } = String.Empty;

        public string Surname { get; set; } = String.Empty;

        public string Legacyname { get; set; } = String.Empty;
        
    }
}