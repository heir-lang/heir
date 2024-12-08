namespace Heir.Syntax
{
    class BiDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        public int Count => _keyToValue.Count;

        private readonly Dictionary<TKey, TValue> _keyToValue = new();
        private readonly Dictionary<TValue, TKey> _valueToKey = new();

        public BiDictionary(IDictionary<TKey, TValue> initialItems)
        {
            foreach (var pair in initialItems)
                Add(pair.Key, pair.Value);
        }

        public TValue? TryGetValue(TKey key)
        {
            TValue? value;
            _keyToValue.TryGetValue(key, out value);
            
            return value;
        }
        public TKey? TryGetKey(TValue value)
        {
            TKey? key;
            _valueToKey.TryGetValue(value, out key);

            return key;
        }
        public TValue GetValue(TKey key) => _keyToValue[key];
        public TKey GetKey(TValue value) => _valueToKey[value];
        public bool Contains(TKey key) => _keyToValue.ContainsKey(key) || _valueToKey.ContainsValue(key);
        public bool Contains(TValue value) => _valueToKey.ContainsKey(value) || _keyToValue.ContainsValue(value);

        private void Add(TKey key, TValue value)
        {
            if (Contains(key) || Contains(value))
                throw new ArgumentException("Duplicate key or value.");

            _keyToValue[key] = value;
            _valueToKey[value] = key;
        }
    }

    internal static class SyntaxFacts
    {
        public static readonly Dictionary<char, int> RadixCodes = new Dictionary<char, int>
        {
            { 'b', 2 },
            { 'o', 8 },
            { 'x', 16 }
        };

        public static readonly BiDictionary<string, SyntaxKind> OperatorMap = new(new Dictionary<string, SyntaxKind>
        {
            { "+", SyntaxKind.Plus },
            { "+=", SyntaxKind.PlusEquals },
            { "++", SyntaxKind.PlusPlus },
            { "-", SyntaxKind.Minus },
            { "-=", SyntaxKind.MinusEquals },
            { "--", SyntaxKind.MinusMinus },
            { "*", SyntaxKind.Star },
            { "*=", SyntaxKind.StarEquals },
            { "/", SyntaxKind.Slash },
            { "/=", SyntaxKind.SlashEquals },
            { "//", SyntaxKind.SlashSlash },
            { "//=", SyntaxKind.SlashSlashEquals },
            { "%", SyntaxKind.Percent },
            { "%=", SyntaxKind.PercentEquals },
            { "^", SyntaxKind.Carat },
            { "^=", SyntaxKind.CaratEquals },
            { "??", SyntaxKind.QuestionQuestion },
            { "??=", SyntaxKind.QuestionQuestionEquals }
        });

        public static readonly BiDictionary<string, SyntaxKind> KeywordMap = new(new Dictionary<string, SyntaxKind>
        {
            { "let", SyntaxKind.LetKeyword },
            { "mut", SyntaxKind.MutKeyword }
        });
    }
}
