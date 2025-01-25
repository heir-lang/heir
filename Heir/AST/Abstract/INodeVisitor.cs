namespace Heir.AST.Abstract;

public struct Void;

public interface INodeVisitor : INodeVisitor<Void>;

public interface INodeVisitor<out T>
    : Expression.IVisitor<T>,
      Statement.IVisitor<T>;