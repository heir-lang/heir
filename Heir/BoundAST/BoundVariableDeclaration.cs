using Heir.Syntax;
using Heir.Types;

namespace Heir.BoundAST
{
    public sealed class BoundVariableDeclaration(BoundIdentifierName name, BoundExpression? initializer, bool isMutable) : BoundStatement
    {
        public override BaseType? Type => throw new NotImplementedException();

        public BoundIdentifierName Name { get; } = name;
        public BoundExpression? Initializer { get; } = initializer;
        public bool IsMutable { get; } = isMutable;

        public override R Accept<R>(Visitor<R> visitor) => visitor.VisitBoundVariableDeclaration(this);
        public override List<Token> GetTokens() => Name.GetTokens().Concat(Initializer?.GetTokens() ?? []).ToList();

        public override void Display(int indent = 0)
        {
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent))}BoundVariableDeclaration(");
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Name -> ");
            Name.Display(0);
            Console.WriteLine(",");
            Console.WriteLine($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Initializer -> {(Initializer == null ? "none" : "")}");
            Initializer?.Display(indent + 2);
            Console.WriteLine(",");
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent + 1))}Mutable: {IsMutable}");
            Console.Write($"{string.Concat(Enumerable.Repeat("  ", indent))})");
        }
    }
}
