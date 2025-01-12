using Heir.Types;

namespace Heir.Tests;

public class TypeTest
{
    [Fact]
    public void AnyType_IsAssignableTo()
    {
        {
            var a = PrimitiveType.Int;
            var b = new AnyType();
            Assert.True(a.IsAssignableTo(b));
        }
        {
            var a = new AnyType();
            var b = PrimitiveType.String;
            Assert.True(a.IsAssignableTo(b));
        }
    }

    [Fact]
    public void SingularTypes_AreAssignableTo()
    {
        var a = new SingularType("MyType");
        var b = new SingularType("MyType");
        Assert.True(a.IsAssignableTo(b));
    }

    [Fact]
    public void PrimitiveTypes_AreAssignableTo()
    {
        var a = PrimitiveType.Int;
        var b = PrimitiveType.Int;
        Assert.True(a.IsAssignableTo(b));
    }
    
    [Fact]
    public void ParenthesizesTypes_AreAssignableTo()
    {
        var a = new ParenthesizedType(PrimitiveType.Int);
        var b = new ParenthesizedType(PrimitiveType.Int);
        Assert.True(a.IsAssignableTo(b));
    }

    [Fact]
    public void UnionTypes_AreAssignableTo()
    {
        {
            var a = PrimitiveType.Int;
            var b = IntrinsicTypes.Number;
            Assert.True(a.IsAssignableTo(b));
        }
        {
            var a = IntrinsicTypes.Number;
            var b = PrimitiveType.Float;
            Assert.True(a.IsAssignableTo(b));
        }
        {
            var a = PrimitiveType.String;
            var b = IntrinsicTypes.StringOrChar;
            Assert.True(a.IsAssignableTo(b));
        }
        {
            var a = IntrinsicTypes.StringOrChar;
            var b = PrimitiveType.Char;
            Assert.True(a.IsAssignableTo(b));
        }
    }
    
    [Fact]
    public void IntersectionTypes_AreAssignableTo()
    {
        {
            var a = new IntersectionType([PrimitiveType.Float, PrimitiveType.Int]);
            var b = new IntersectionType([PrimitiveType.Float, PrimitiveType.Int, PrimitiveType.String]);
            Assert.False(a.IsAssignableTo(b));
            Assert.True(b.IsAssignableTo(a));
        }
        {
            var a = new IntersectionType([IntrinsicTypes.Number, PrimitiveType.Int]);
            var b = PrimitiveType.String;
            Assert.False(a.IsAssignableTo(b));
        }
        {
            var a = new IntersectionType([PrimitiveType.Int, PrimitiveType.Float]);
            Assert.True(a.IsAssignableTo(a));
        }
    }

    [Fact]
    public void LiteralTypes_AreAssignableTo()
    {
        var a = new LiteralType("hello");
        var b = new LiteralType("hello");
        Assert.True(a.IsAssignableTo(b));
    }
}
