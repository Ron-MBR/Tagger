namespace Tagger.Data
{
    public class DynamicCounterStore
    {
        private readonly Dictionary<long, int> _counters = new Dictionary<long, int>();
        private readonly object _lock = new object();

        public bool Add(long ChatId)
        {
            lock (_lock)
            {
                return _counters.TryAdd(ChatId, 1);
            }
        }

        public int Get(long ChatId)
        {
            lock (_lock)
            {
                return _counters.GetValueOrDefault(ChatId,0);
            }
        }
        
        public bool Remove(long ChatId)
        {
            lock (_lock)
            {
                return _counters.Remove(ChatId);
            }
        }

        public bool Push(long ChatId)
        {
            lock (_lock)
            {
                int i = _counters.GetValueOrDefault(ChatId,0);
                if (i == 0) return false;
                i++;
                _counters[ChatId] = i;
                return true;
            }
        }
    }
}