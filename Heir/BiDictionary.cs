namespace Heir
{
    public class BiDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        public readonly Dictionary<TKey, TValue> Forward = new();
        public readonly Dictionary<TValue, TKey> Reverse = new();
        public int Count => Forward.Count;


        public BiDictionary(IDictionary<TKey, TValue> initialItems)
        {
            foreach (var pair in initialItems)
                Add(pair.Key, pair.Value);
        }

        public TValue? TryGetValue(TKey key)
        {
            TValue? value;
            Forward.TryGetValue(key, out value);

            return value;
        }
        public TKey? TryGetKey(TValue value)
        {
            TKey? key;
            Reverse.TryGetValue(value, out key);

            return key;
        }
        public TValue GetValue(TKey key) => Forward[key];
        public TKey GetKey(TValue value) => Reverse[value];
        public bool Contains(TKey key) => Forward.ContainsKey(key) || Reverse.ContainsValue(key);
        public bool Contains(TValue value) => Reverse.ContainsKey(value) || Forward.ContainsValue(value);

        private void Add(TKey key, TValue value)
        {
            if (Contains(key) || Contains(value))
                throw new ArgumentException("Duplicate key or value.");

            Forward[key] = value;
            Reverse[value] = key;
        }
    }
}
