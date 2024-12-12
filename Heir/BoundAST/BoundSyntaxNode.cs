using Heir.Types;

namespace Heir.AST
{
    public abstract class BoundStatement : BoundSyntaxNode
    {
        public abstract BaseType? Type { get; }

        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<R>
        {
            public R VisitBoundSyntaxTree(BoundSyntaxTree boundBlock);
            public R VisitBoundBlockStatement(BoundBlock boundBlock);
        }
    }

    public abstract class BoundExpression : BoundSyntaxNode
    {
        public abstract BaseType Type { get; }

        public abstract R Accept<R>(Visitor<R> visitor);

        public interface Visitor<R>
        {
            
        }
    }

    public abstract class BoundSyntaxNode : SyntaxNode
    {
    }
}
