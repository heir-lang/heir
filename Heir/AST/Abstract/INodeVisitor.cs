namespace Heir.AST.Abstract;

public interface INodeVisitor<out T>
    : Expression.IVisitor<T>,
      Statement.IVisitor<T>;