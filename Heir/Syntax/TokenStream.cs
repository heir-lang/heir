using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Heir.Syntax;

public sealed class TokenStream(DiagnosticBag diagnostics, List<Token> tokens) : IEnumerable<Token>
{
    public readonly DiagnosticBag Diagnostics = diagnostics;
    /// <summary>Whether the position has exceeded the amount of tokens in the stream.</summary>
    public bool IsAtEnd => Position >= tokens.Count;
    /// <summary>The token at the current position.</summary>
    public Token Current => Peek(0)!;
    /// <summary>The token at the previous position.</summary>
    public Token? Previous => Peek(-1);
    /// <summary>The position (amount of tokens read) in the stream.</summary>
    public int Position { get; private set; }
    
    /// <returns>An identical <see cref="TokenStream"/> but with all <see cref="TriviaToken"/>s removed</returns>
    public TokenStream WithoutTrivia() =>
        new(Diagnostics, tokens.FindAll(token => !token.IsKind(SyntaxKind.Trivia)));
    
    /// <returns>An identical <see cref="TokenStream"/> but with all <see cref="TriviaToken"/>s except semicolons removed</returns>
    public TokenStream WithoutTriviaExceptSemicolons() =>
        new(Diagnostics, tokens.FindAll(token => !token.IsKind(SyntaxKind.Trivia) || token is TriviaToken { TriviaKind: TriviaKind.Semicolons }));

    /// <summary>
    /// Checks if the current token is of the given <see cref="SyntaxKind"/>.
    /// If it is then it consumes the token and return true, otherwise returns false.
    /// </summary>
    /// /// <param name="kind">The <see cref="SyntaxKind"/> to check for</param>
    public bool Match(SyntaxKind kind) => Match(kind, out _);

    /// <summary>
    /// Checks if the current token is of the given <see cref="SyntaxKind"/>.
    /// If it is then it consumes the token and return true, otherwise returns false.
    /// </summary>
    /// <param name="kind">The <see cref="SyntaxKind"/> to check for</param>
    /// <param name="matchedToken">The consumed token (if the method returns true, otherwise null)</param>
    public bool Match(SyntaxKind kind, [MaybeNullWhen(false)] out Token matchedToken)
    {
        var isMatch = Check(kind);
        matchedToken = null;
        if (isMatch)
            matchedToken = Advance();

        return isMatch;
    }

    /// <returns>Whether the following sequence of tokens, starting at <see cref="startOffset"/>, matches the given <see cref="HashSet{T}"/> of tokens in order</returns>
    public bool CheckSequential(HashSet<SyntaxKind> kinds, int startOffset = 0)
    {
        var offset = startOffset;
        return kinds.All(kind => Check(kind, offset++));
    }

    /// <returns>Whether the token at <see cref="offset"/> is any of the <see cref="SyntaxKind"/>s in the given <see cref="HashSet{T}"/></returns>
    public bool CheckSet(HashSet<SyntaxKind> kinds, int offset = 0) =>
        kinds.Any(kind => Check(kind, offset));
    
    /// <returns>Whether the token at <see cref="offset"/> is of the given <see cref="SyntaxKind"/></returns>
    public bool Check(SyntaxKind kind, int offset = 0)
    {
        var token = Peek(offset);
        return token != null && token.IsKind(kind);
    }

    /// <summary>Consumes any <see cref="SyntaxFacts.TypeSyntaxes"/>.</summary>
    /// <returns>The consumed type token</returns>
    public Token? ConsumeType()
    {
        var token = Advance();
        if (SyntaxFacts.TypeSyntaxes.Any(typeKind => token.IsKind(typeKind)))
            return token;

        Diagnostics.Error(DiagnosticCode.H004B,
            $"Expected type, got '{token?.Kind.ToString() ?? "EOF"}'",
            token ?? Peek(-2)!);
            
        return null;
    }

    /// <summary>
    /// Advances, and if it does not match the given <see cref="SyntaxKind"/>,
    /// throw an error because we received an unexpected token.
    /// </summary>
    /// <param name="kind">The expected <see cref="SyntaxKind"/></param>
    /// <returns>The consumed token</returns>
    public Token? Consume(SyntaxKind kind)
    {
        var token = Advance();
        if (token != null && token.IsKind(kind))
            return token;
        
        var invalidToken = token ?? Peek(-2) ?? Peek(-3)!;
        var got = SyntaxFacts.OperatorMap.Contains(invalidToken.Kind)
            ? SyntaxFacts.OperatorMap.GetKey(invalidToken.Kind)
            : SyntaxFacts.KeywordMap.Contains(invalidToken.Kind)
                ? SyntaxFacts.KeywordMap.GetKey(invalidToken.Kind)
                : invalidToken.Kind.ToString();
                
        Diagnostics.Error(DiagnosticCode.H004,
            $"Expected {kind}, got '{got}'",
            invalidToken);

        return token;
    }

    /// <summary>Increments the position in the <see cref="TokenStream"/>.</summary>
    /// <returns>The token before incrementing the position</returns>
    public Token Advance()
    {
        var token = Current;
        Position++;
        return token;
    }
    
    /// <returns>The token at the given offset away from <see cref="Position"/></returns>
    public Token? Peek(int offset) => tokens.ElementAtOrDefault(Position + offset);

    public override string ToString()
    {
        var result = new StringBuilder();
        foreach (var token in this)
            result.AppendLine(token.ToString());

        return result.ToString().TrimEnd();
    }

    public IEnumerator<Token> GetEnumerator() => ((IEnumerable<Token>)tokens).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => tokens.GetEnumerator();
}