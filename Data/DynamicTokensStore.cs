using System.Reflection.Metadata;

namespace Tagger.Data
{
    public class DynamicTokensStore
    {
        private readonly Dictionary<string, long> _tokens = new Dictionary<string, long>();
        private readonly object _lock = new object();
        
        public bool Add(string token, long chatId)
        {
            lock (_lock)
            {
                return _tokens.TryAdd(token, chatId);
            }
        }

        public long Get(string token)
        {
            lock (_lock)
            {
                return _tokens.GetValueOrDefault(token,0);
            }
        }

        public bool Remove(string token)
        {
            lock (_lock)
            {
                return _tokens.Remove(token);
            }
        }
    }
}