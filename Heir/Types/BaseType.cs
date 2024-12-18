using Heir.Syntax;
using Heir.AST;

namespace Heir.Types
{
    public abstract class BaseType
    {
        public abstract TypeKind Kind { get; }

        public abstract string ToString(bool colors = false);

        public static BaseType FromTypeRef(TypeRef typeRef)
        {
            switch (typeRef)
            {
                case AST.SingularType singularType:
                    {
                        if (singularType.Token.IsKind(SyntaxKind.Identifier))
                            return new SingularType(singularType.Token.Text);

                        return SyntaxFacts.PrimitiveTypeMap[singularType.Token.Kind];
                    }
                case AST.UnionType unionType:
                    {
                        var types = unionType.Types.ConvertAll(FromTypeRef);
                        return new UnionType(types);
                    }
            }

            return PrimitiveType.None;
        }

        public bool IsAssignableTo(BaseType other)
        {
            if (this is AnyType || other is AnyType)
                return true;
            else if (this is UnionType union)
                return union.Types.Any(type => type.IsAssignableTo(other));
            else if (other is UnionType)
                return other.IsAssignableTo(this);
            else if (this is LiteralType literal && other is LiteralType otherLiteral)
                return literal.Value == otherLiteral.Value;
            else if (this is SingularType singular && other is SingularType otherSingular)
                return singular.Name == otherSingular.Name;

            return false;
        }
    }
}
