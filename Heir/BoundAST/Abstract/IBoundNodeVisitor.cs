namespace Heir.BoundAST.Abstract;

public interface IBoundNodeVisitor<out T>
    : BoundExpression.IVisitor<T>,
      BoundStatement.IVisitor<T>;