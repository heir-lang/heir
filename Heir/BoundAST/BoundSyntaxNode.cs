using Heir.Types;

namespace Heir.AST
{
    public abstract class BoundStatement : BoundSyntaxNode
    {
        public abstract BaseType? Type { get; }

        public abstract class Visitor<R>
        {
            
        }
    }

    public abstract class BoundExpression : BoundSyntaxNode
    {
        public abstract BaseType Type { get; }

        public abstract class Visitor<R>
        {
            
        }
    }

    public abstract class BoundSyntaxNode : SyntaxNode
    {
    }
}
