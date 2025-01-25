namespace Heir.BoundAST.Abstract;

public struct Void;

public interface IBoundNodeVisitor : IBoundNodeVisitor<Void>;

public interface IBoundNodeVisitor<out T>
    : BoundExpression.IVisitor<T>,
      BoundStatement.IVisitor<T>;