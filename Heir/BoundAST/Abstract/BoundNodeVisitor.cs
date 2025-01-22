namespace Heir.BoundAST.Abstract;

public interface BoundNodeVisitor<out T>
    : BoundExpression.Visitor<T>, BoundStatement.Visitor<T>;