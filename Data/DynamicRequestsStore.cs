namespace Tagger.Data
{
    public class DynamicRequestsStore<T> where T: notnull
    {
        private readonly Dictionary<T, int> _requests = new Dictionary<T, int>();
        private readonly object _lock = new object();

        public bool Add(T Id, int Code)
        {
            lock (_lock)
            {
                return _requests.TryAdd(Id, Code);
            }
        }

        public int Get(T Id)
        {
            lock (_lock)
            {
                return _requests.GetValueOrDefault(Id,0);
            }
        }

        public bool Remove(T Id)
        {
            lock (_lock)
            {
                return _requests.Remove(Id);
            }
        }
    }
}