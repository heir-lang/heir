namespace Heir.AST.Abstract;

public interface NodeVisitor<out T>
    : Expression.Visitor<T>, Statement.Visitor<T>;