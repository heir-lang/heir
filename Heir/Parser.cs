using Heir.Syntax;
using Heir.AST;
using Heir.AST.Abstract;
using Heir.Diagnostics;
using FunctionType = Heir.AST.FunctionType;
using IntersectionType = Heir.AST.IntersectionType;
using ParenthesizedType = Heir.AST.ParenthesizedType;
using SingularType = Heir.AST.SingularType;
using UnionType = Heir.AST.UnionType;

namespace Heir;

public sealed class Parser(TokenStream tokenStream)
{
    public TokenStream Tokens { get; } = tokenStream.WithoutTriviaExceptSemicolons();

    private readonly DiagnosticBag _diagnostics = tokenStream.Diagnostics;
    private int _enumMemberCount;

    public SyntaxTree ParseWithCompileTimeMacros(bool resolveBeforeMacros = true)
    {
        var tree = Parse();
        if (resolveBeforeMacros)
        {
            var resolver = new Resolver(_diagnostics, tree);
            resolver.Resolve();

            if (_diagnostics.HasErrors)
                return tree;
        }
        
        var macroEvaluator = new CompileTimeMacroEvaluator(tree);
        return macroEvaluator.Evaluate();
    }

    public SyntaxTree Parse()
    {
        var statements = new List<Statement>();
        while (!Tokens.IsAtEnd)
        {
            var statement = ParseStatement();
            statements.Add(statement);
        }

        return new SyntaxTree(statements, _diagnostics);
    }

    private List<Statement> ParseStatementsUntil(Func<bool> predicate)
    {
        var statements = new List<Statement>();
        while (!predicate() && !Tokens.IsAtEnd)
            statements.Add(ParseStatement());

        return statements;
    }

    private Statement ParseStatement()
    {
        Statement statement;
        if (Tokens.Match(SyntaxKind.LetKeyword))
        {
            statement = ParseVariableDeclaration();
            goto ConsumeSemicolons;
        }
        
        if (Tokens.Match(SyntaxKind.FnKeyword))
        {
            statement = ParseFunctionDeclaration();
            goto ConsumeSemicolons;
        }
        
        if (Tokens.Match(SyntaxKind.ReturnKeyword))
        {
            statement = ParseReturnStatement();
            goto ConsumeSemicolons;
        }
        
        if (Tokens.Match(SyntaxKind.BreakKeyword, out var breakKeyword))
        {
            statement = new Break(breakKeyword);
            goto ConsumeSemicolons;
        }
        
        if (Tokens.Match(SyntaxKind.ContinueKeyword, out var continueKeyword))
        {
            statement = new Continue(continueKeyword);
            goto ConsumeSemicolons;
        }
        
        if (Tokens.Match(SyntaxKind.InterfaceKeyword, out var interfaceKeyword))
        {
            statement = ParseInterfaceDeclaration(interfaceKeyword);
            goto ConsumeSemicolons;
        }

        var isInlineEnum = false;
        if (Tokens.Check(SyntaxKind.InlineKeyword) && Tokens.Check(SyntaxKind.EnumKeyword, 1))
        {
            isInlineEnum = true;
            Tokens.Advance();
        }
        
        if (Tokens.Match(SyntaxKind.EnumKeyword, out var enumKeyword))
        {
            statement = ParseEnumDeclaration(enumKeyword, isInlineEnum);
            goto ConsumeSemicolons;
        }
        
        if (Tokens.Match(SyntaxKind.IfKeyword))
        {
            statement = ParseIfStatement();
            goto ConsumeSemicolons;
        }
        
        if (Tokens.Match(SyntaxKind.WhileKeyword))
        {
            statement = ParseWhileStatement();
            goto ConsumeSemicolons;
        }
        
        if (Tokens.Match(SyntaxKind.LBrace))
        {
            var token = Tokens.Previous!.WithKind(SyntaxKind.ObjectLiteral);
            if (Tokens.Match(SyntaxKind.RBrace))
            {
                statement = new ExpressionStatement(new ObjectLiteral(token, []));
                goto ConsumeSemicolons;
            }

            if (Tokens.CheckSequential([SyntaxKind.Identifier, SyntaxKind.Colon]))
            {
                statement = new ExpressionStatement(ParseObject(token));
                goto ConsumeSemicolons;
            }

            if (Tokens.Check(SyntaxKind.LBracket))
            {
                var offset = 1;
                while (!Tokens.Check(SyntaxKind.RBracket, ++offset))
                    offset++;

                if (Tokens.Check(SyntaxKind.Colon, offset + 1))
                {
                    statement = new ExpressionStatement(ParseObject(token));
                    goto ConsumeSemicolons;
                }
            }

            statement = ParseBlock();
            goto ConsumeSemicolons;
        }

        var expression = ParseExpression();
        statement = new ExpressionStatement(expression);
        
        ConsumeSemicolons: ConsumeSemicolons();
        
        return statement;
    }

    private void ConsumeSemicolons()
    {
        while (Tokens.Current is TriviaToken { TriviaKind: TriviaKind.Semicolons })
            Tokens.Advance();
    }

    private Block ParseBlock()
    {
        var statements = ParseStatementsUntil(() => Tokens.Match(SyntaxKind.RBrace));
        return new Block(statements);
    }

    private ArrayLiteral ParseArray(Token bracket)
    {
        var elements = new List<Expression> { ParseExpression() };
        while (Tokens.Match(SyntaxKind.Comma) && !Tokens.Check(SyntaxKind.RBracket))
            elements.Add(ParseExpression());

        Tokens.Consume(SyntaxKind.RBracket);
        return new ArrayLiteral(bracket, elements);
    }

    private ObjectLiteral ParseObject(Token brace)
    {
        var keyValuePairs = new List<KeyValuePair<Expression, Expression>> { ParseObjectKeyValuePair() };
        while (Tokens.Match(SyntaxKind.Comma) && !IsAtEndOfBlock())
            keyValuePairs.Add(ParseObjectKeyValuePair());
            
        Tokens.Consume(SyntaxKind.RBrace);
        return new ObjectLiteral(brace, new(keyValuePairs));
    }

    private KeyValuePair<Expression, Expression> ParseObjectKeyValuePair()
    {
        Expression key;
        if (Tokens.Match(SyntaxKind.LBracket))
        {
            key = ParseExpression();
            Tokens.Consume(SyntaxKind.RBracket);
        }
        else
        {
            var identifier = Tokens.Consume(SyntaxKind.Identifier)!;
            key = new Literal(TokenFactory.StringFromIdentifier(identifier));
        }

        Tokens.Consume(SyntaxKind.Colon);
        var value = ParseExpression();
        return new(key, value);
    }

    private If ParseIfStatement()
    {
        var keyword = Tokens.Previous!;
        var condition = ParseExpression();
        var body = ParseStatement();

        Statement? elseBranch = null;
        if (Tokens.Match(SyntaxKind.ElseKeyword))
            elseBranch = ParseStatement();
        
        return new If(keyword, condition, body, elseBranch);
    }
    
    private While ParseWhileStatement()
    {
        var keyword = Tokens.Previous!;
        var condition = ParseExpression();
        var body = ParseStatement();
        
        return new While(keyword, condition, body);
    }

    private Statement ParseInterfaceDeclaration(Token keyword)
    {
        var identifier = Tokens.Consume(SyntaxKind.Identifier);
        if (identifier == null)
            return new NoOpStatement();

        var containsBody = Tokens.Match(SyntaxKind.LBrace, out var braceToken);
        if (!containsBody)
            return new InterfaceDeclaration(keyword, identifier, []);
        
        var fields = new HashSet<InterfaceField>();
        while (!IsAtEndOfBlock())
        {
            var field = ParseInterfaceField();
            if (field == null)
                return new NoOpStatement();
            
            if (fields.Add(field)) continue;
            
            _diagnostics.Error(DiagnosticCode.H024, $"Interface '{identifier.Text}' contains duplicate field '{field.Identifier.Text}'", field);
            break;
        }
        if (Tokens.Consume(SyntaxKind.RBrace) == null)
            return new NoOpStatement();
            
        if (fields.Count == 0)
            _diagnostics.Warn(DiagnosticCode.H020,
                $"Empty interface with body, convert to 'interface {identifier.Text};'",
                braceToken!);

        return new InterfaceDeclaration(keyword, identifier, fields.ToList());
    }

    private InterfaceField? ParseInterfaceField()
    {
        var isMutable = Tokens.Match(SyntaxKind.MutKeyword);
        var identifier = Tokens.Consume(SyntaxKind.Identifier);
        if (identifier == null)
            return null;
            
        Tokens.Consume(SyntaxKind.Colon);
        var type = ParseType();
        ConsumeSemicolons();
        
        return new InterfaceField(identifier, type, isMutable);
    }
    
    private Statement ParseEnumDeclaration(Token keyword, bool isInline)
    {
        _enumMemberCount = 0;
        var identifier = Tokens.Consume(SyntaxKind.Identifier);
        if (identifier == null)
            return new NoOpStatement();

        if (Tokens.Consume(SyntaxKind.LBrace) == null)
            return new NoOpStatement();

        var members = new HashSet<EnumMember>();
        var firstMember = ParseEnumMember();
        if (firstMember == null)
            return new NoOpStatement();
        
        members.Add(firstMember);
        while (Tokens.Match(SyntaxKind.Comma) && !IsAtEndOfBlock())
        {
            var member = ParseEnumMember();
            if (member == null)
                return new NoOpStatement();
            
            if (members.Add(member)) continue;

            _diagnostics.Error(DiagnosticCode.H024, $"Enum '{identifier.Text}' contains duplicate member '{member.Name.Token.Text}'", member.Name);
            break;
        }
        if (Tokens.Consume(SyntaxKind.RBrace) == null)
            return new NoOpStatement();

        return new EnumDeclaration(keyword, new IdentifierName(identifier), members, isInline);
    }
    
    private EnumMember? ParseEnumMember()
    {
        var identifier = Tokens.Consume(SyntaxKind.Identifier);
        if (identifier == null)
            return null;
        
        var valueLiteral = new Literal(TokenFactory.IntLiteral(_enumMemberCount, identifier));
        if (Tokens.Match(SyntaxKind.Equals) && Tokens.CheckSet([SyntaxKind.IntLiteral, SyntaxKind.StringLiteral, SyntaxKind.NameofKeyword]))
        {
            var valueToken = Tokens.Advance();
            if (valueToken == null)
                return null;
            
            valueLiteral = new Literal(valueToken);
        }
        
        if (valueLiteral is { Token.Value: int value })
            _enumMemberCount = value + 1;
        else if (valueLiteral is not { Token.Value: not int })
            _enumMemberCount++;
        
        return new EnumMember(new IdentifierName(identifier), valueLiteral);
    }

    private Return ParseReturnStatement()
    {
        var keyword = Tokens.Previous!;
        var noExpression = Tokens.Check(SyntaxKind.RBrace) ||
                                Tokens.Current is TriviaToken
                                {
                                    TriviaKind: TriviaKind.EOF or TriviaKind.Semicolons
                                } ||
                                Tokens.IsAtEnd;
        
        var expression = noExpression
            ? new Literal(TokenFactory.NoneLiteral())
            : ParseExpression();
        
        return new Return(keyword, expression);
    }
    
    private Statement ParseFunctionDeclaration()
    {
        var keyword = Tokens.Previous!;
        if (!Tokens.Match(SyntaxKind.Identifier, out var identifier))
        {
            _diagnostics.Error(DiagnosticCode.H004C, $"Expected identifier after 'fn', got {Tokens.Current}", Tokens.Previous!);
            return new NoOpStatement();
        }
        
        var typeParameters = new List<TypeParameter>();
        if (Tokens.Match(SyntaxKind.LT))
        {
            while (!Tokens.Check(SyntaxKind.GT) && !Tokens.IsAtEnd)
            {
                var expression = ParseTypeParameter();
                if (expression is not TypeParameter typeParameter) continue;
                
                typeParameters.Add(typeParameter);
                Tokens.Match(SyntaxKind.Comma);
            }
            Tokens.Consume(SyntaxKind.GT);
        }

        var parameters = new List<Parameter>();
        if (Tokens.Match(SyntaxKind.LParen))
        {
            while (!Tokens.Check(SyntaxKind.RParen) && !Tokens.IsAtEnd)
            {
                var expression = ParseParameter();
                if (expression is not Parameter parameter) continue;
                
                parameters.Add(parameter);
                Tokens.Match(SyntaxKind.Comma);
            }
            Tokens.Consume(SyntaxKind.RParen);
        }
        
        TypeRef? returnType = null;
        if (Tokens.Match(SyntaxKind.Colon))
            returnType = ParseType();

        Block body;
        if (Tokens.Match(SyntaxKind.DashRArrow))
            body = new Block([new Return(TokenFactory.Keyword(SyntaxKind.ReturnKeyword), ParseExpression())]);
        else
        {
            Tokens.Consume(SyntaxKind.LBrace);
            body = ParseBlock();
        }

        return new FunctionDeclaration(keyword, new IdentifierName(identifier), parameters, typeParameters, body, returnType);
    }

    private Statement ParseVariableDeclaration()
    {
        var isInline = Tokens.Match(SyntaxKind.InlineKeyword);
        var isMutable = Tokens.Match(SyntaxKind.MutKeyword);
        if ((isInline && isMutable) || (isMutable && Tokens.Check(SyntaxKind.InlineKeyword))) {
            _diagnostics.Error(DiagnosticCode.H021, "Variable declaration cannot be marked as both inline and mutable", Tokens.Previous!);
            return new NoOpStatement();
        }
        
        if (!Tokens.Match(SyntaxKind.Identifier, out var identifier))
        {
            _diagnostics.Error(DiagnosticCode.H004C, $"Expected identifier after 'let', got {Tokens.Current}", Tokens.Previous!);
            return new NoOpStatement();
        }

        TypeRef? type = null;
        if (Tokens.Match(SyntaxKind.Colon))
            type = ParseType();

        Expression? initializer = null;
        if (Tokens.Match(SyntaxKind.Equals))
            initializer = ParseExpression();

        if (isInline && (initializer == null ||
                         (initializer is Literal literal && literal.Token.IsKind(SyntaxKind.NoneKeyword))))
        {
            _diagnostics.Error(DiagnosticCode.H022, $"Inlined variable '{identifier.Text}' must be initialized and non-null", identifier);
        }

        if (initializer == null && type == null)
        {
            _diagnostics.Error(DiagnosticCode.H012, $"Cannot infer type of variable '{identifier.Text}', please add an explicit type or initializer", identifier);
            return new NoOpStatement();
        }

        return new VariableDeclaration(new IdentifierName(identifier), initializer, type, isMutable, isInline);
    }

    private Expression ParseParameter()
    {
        var identifier = Tokens.Consume(SyntaxKind.Identifier);
        if (identifier == null)
            return new NoOp();
        
        TypeRef? type = null;
        if (Tokens.Match(SyntaxKind.Colon))
            type = ParseType();
        
        Literal? initializer = null;
        if (Tokens.Match(SyntaxKind.Equals))
        {
            var expression = ParsePrimary();
            if (expression is not Literal literal)
            {
                _diagnostics.Error(DiagnosticCode.H016, "Parameter initializers must be literals", identifier);
                return new NoOp();
            }
            
            initializer = literal;
        }
        
        if (initializer == null && type == null)
        {
            _diagnostics.Error(DiagnosticCode.H012, $"Cannot infer type of parameter '{identifier.Text}', please add an explicit type or initializer", identifier);
            return new NoOp();
        }
        
        return new Parameter(new IdentifierName(identifier), type, initializer);
    }
    
    private Expression ParseTypeParameter()
    {
        var identifier = Tokens.Consume(SyntaxKind.Identifier);
        if (identifier == null)
            return new NoOp();
        
        TypeRef? baseType = null;
        if (Tokens.Match(SyntaxKind.Colon))
            baseType = ParseType();
        
        TypeRef? initializer = null;
        if (Tokens.Match(SyntaxKind.Equals))
            initializer = ParseType();

        return new TypeParameter(new IdentifierName(identifier), baseType, initializer);
    }

    private TypeRef ParseType()
    {
        var type = ParseUnionType();
        if (type is ParenthesizedType { Type: not SingularType }) // or LiteralType
            UnnecessaryParentheses(type);
            
        return type;
    }
        
    private TypeRef ParseUnionType()
    {
        var left = ParseIntersectionType();
        if (Tokens.Match(SyntaxKind.Pipe))
        {
            var right = ParseUnionType();
            return new UnionType([left, right]);
        }

        return left;
    }
        
    private TypeRef ParseIntersectionType()
    {
        var left = ParseArrayType();
        if (Tokens.Match(SyntaxKind.Question, out var questionToken))
            left = new UnionType([
                left,
                new SingularType(TokenFactory.NoneLiteral(questionToken))]
            );
        
        if (Tokens.Match(SyntaxKind.Ampersand))
        {
            if (left is UnionType)
                left = new ParenthesizedType(left);
            
            var right = ParseIntersectionType();
            return new IntersectionType([left, right]);
        }

        return left;
    }

    private TypeRef ParseArrayType()
    {
        var elementType = ParseParenthesizedOrFunctionType();
        if (!Tokens.Match(SyntaxKind.LBracket))
            return elementType;
        
        Tokens.Consume(SyntaxKind.RBracket);
        return new ArrayType(elementType);
    }

    private TypeRef ParseParenthesizedOrFunctionType()
    {
        if (!Tokens.Check(SyntaxKind.LT) && !Tokens.Check(SyntaxKind.LParen))
            return ParseSingularType();

        var typeParameters = new List<TypeParameter>();
        if (Tokens.Match(SyntaxKind.LT))
        {
            while (!Tokens.Check(SyntaxKind.GT) && !Tokens.IsAtEnd)
            {
                var expression = ParseTypeParameter();
                if (expression is not TypeParameter typeParameter) continue;
                
                typeParameters.Add(typeParameter);
                Tokens.Match(SyntaxKind.Comma);
            }
            Tokens.Consume(SyntaxKind.GT);
        }
        
        Tokens.Consume(SyntaxKind.LParen);
        if (!IsFunctionType())
        {
            var innerType = ParseType();
            Tokens.Consume(SyntaxKind.RParen);

            if (innerType is SingularType) //  || innerType is LiteralType
                UnnecessaryParentheses(innerType);

            return new ParenthesizedType(innerType);
        }

        var parameterTypes = new Dictionary<string, TypeRef>();
        while (!Tokens.Check(SyntaxKind.RParen) && !Tokens.IsAtEnd)
        {
            var identifier = Tokens.Consume(SyntaxKind.Identifier);
            if (identifier == null)
                return new NoOpType();

            Tokens.Consume(SyntaxKind.Colon);
            var type = ParseType();
            parameterTypes.Add(identifier.Text, type);
            Tokens.Match(SyntaxKind.Comma);
        }

        Tokens.Consume(SyntaxKind.RParen);
        Tokens.Consume(SyntaxKind.DashRArrow);
        var returnType = ParseType();

        return new FunctionType(parameterTypes, typeParameters, returnType);
    }

    private SingularType ParseSingularType()
    {
        var token = Tokens.ConsumeType();
        return new SingularType(token ?? Tokens.Previous!);
    }

    private bool IsInvocation(int startOffset = 0)
    {
        var offset = startOffset;
        if (Tokens.Check(SyntaxKind.LT, offset))
        {
            while (!Tokens.Check(SyntaxKind.GT, offset++) && Tokens.Peek(offset) != null)
            {
            }
        }

        if (!Tokens.Check(SyntaxKind.LParen, offset++))
            return false;
        
        while (!Tokens.Check(SyntaxKind.RParen, offset++) && Tokens.Peek(offset) != null)
        {
        }

        return Tokens.Check(SyntaxKind.RParen, offset - 1);
    }

    private bool IsFunctionType(int startOffset = 0)
    {
        var offset = startOffset;
        if (Tokens.Check(SyntaxKind.LParen, offset))
            return false;
        
        while (!Tokens.Check(SyntaxKind.RParen, offset++) && Tokens.Peek(offset) != null)
        {
        }
        
        return Tokens.Check(SyntaxKind.DashRArrow, offset);
    }
    
    private Invocation ParseInvocation(Expression callee)
    {
        var typeArguments = ParseTypeArguments();
        var arguments = ParseArguments();
        return new Invocation(callee, arguments, typeArguments);
    }
    
    private ElementAccess ParseElementAccess(Expression expression)
    {
        var indexExpression = ParseExpression();
        Tokens.Consume(SyntaxKind.RBracket);
        
        return new ElementAccess(expression, indexExpression);
    }
    
    private MemberAccess ParseMemberAccess(Expression expression)
    {
        var name = new IdentifierName(Tokens.Consume(SyntaxKind.Identifier)!);
        return new MemberAccess(expression, name);
    }
    
    private List<TypeRef> ParseTypeArguments()
    {
        var typeArguments = new List<TypeRef>();
        if (!Tokens.Match(SyntaxKind.LT))
            return typeArguments;
        
        while (!Tokens.Check(SyntaxKind.GT) && !Tokens.IsAtEnd)
        {
            typeArguments.Add(ParseType());
            Tokens.Match(SyntaxKind.Comma);
        }

        Tokens.Consume(SyntaxKind.GT);
        return typeArguments;
    }

    private List<Expression> ParseArguments()
    {
        Tokens.Consume(SyntaxKind.LParen);
        
        var arguments = new List<Expression>();
        while (!Tokens.Check(SyntaxKind.RParen) && !Tokens.IsAtEnd)
        {
            arguments.Add(ParseExpression());
            Tokens.Match(SyntaxKind.Comma);
        }

        Tokens.Consume(SyntaxKind.RParen);
        return arguments;
    }

    private Expression ParseExpression() => ParseAssignment();

    private Expression ParseAssignment()
    {
        var left = ParseLogicalOr();
        if (Tokens.Match(SyntaxKind.Equals) ||
            Tokens.Match(SyntaxKind.PlusEquals) ||
            Tokens.Match(SyntaxKind.MinusEquals) ||
            Tokens.Match(SyntaxKind.StarEquals) ||
            Tokens.Match(SyntaxKind.SlashEquals) ||
            Tokens.Match(SyntaxKind.SlashSlashEquals) ||
            Tokens.Match(SyntaxKind.PercentEquals) ||
            Tokens.Match(SyntaxKind.CaratEquals) ||
            Tokens.Match(SyntaxKind.AmpersandEquals) ||
            Tokens.Match(SyntaxKind.PipeEquals) ||
            Tokens.Match(SyntaxKind.TildeEquals) ||
            Tokens.Match(SyntaxKind.AmpersandAmpersandEquals) ||
            Tokens.Match(SyntaxKind.PipePipeEquals) ||
            Tokens.Match(SyntaxKind.LArrowLArrowEquals) ||
            Tokens.Match(SyntaxKind.RArrowRArrowEquals))
        {
            if (left is not AssignmentTarget assignmentTarget)
            {
                _diagnostics.Error(DiagnosticCode.H006B, "Invalid assignment target, expected identifier or member access", left.GetFirstToken());
                return new NoOp();
            }

            var op = Tokens.Previous!;
            var right = ParseAssignment();
            return new AssignmentOp(assignmentTarget, op, right);
        }

        return left;
    }

    private Expression ParseLogicalOr()
    {
        var left = ParseLogicalAnd();
        while (Tokens.Match(SyntaxKind.PipePipe))
        {
            var op = Tokens.Previous!;
            var right = ParseLogicalAnd();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseLogicalAnd()
    {
        var left = ParseComparison();
        while (Tokens.Match(SyntaxKind.AmpersandAmpersand))
        {
            var op = Tokens.Previous!;
            var right = ParseComparison();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseComparison()
    {
        var left = ParseBitwiseXor();
        while (Tokens.Match(SyntaxKind.EqualsEquals) ||
               Tokens.Match(SyntaxKind.BangEquals) ||
               Tokens.Match(SyntaxKind.LT) ||
               Tokens.Match(SyntaxKind.LTE) ||
               Tokens.Match(SyntaxKind.GT) ||
               Tokens.Match(SyntaxKind.GTE))
        {
            var op = Tokens.Previous!;
            var right = ParseBitwiseXor();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseBitwiseXor()
    {
        var left = ParseBitwiseOr();
        while (Tokens.Match(SyntaxKind.Tilde))
        {
            var op = Tokens.Previous!;
            var right = ParseBitwiseOr();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseBitwiseOr()
    {
        var left = ParseBitwiseAnd();
        while (Tokens.Match(SyntaxKind.Pipe))
        {
            var op = Tokens.Previous!;
            var right = ParseBitwiseAnd();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseBitwiseAnd()
    {
        var left = ParseBitShift();
        while (Tokens.Match(SyntaxKind.Ampersand))
        {
            var op = Tokens.Previous!;
            var right = ParseBitShift();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }
    
    private Expression ParseBitShift()
    {
        var left = ParseAddition();
        while (Tokens.Match(SyntaxKind.RArrowRArrow) || Tokens.Match(SyntaxKind.LArrowLArrow))
        {
            var op = Tokens.Previous!;
            var right = ParseAddition();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseAddition()
    {
        var left = ParseMultiplication();
        while (Tokens.Match(SyntaxKind.Plus) || Tokens.Match(SyntaxKind.Minus))
        {
            var op = Tokens.Previous!;
            var right = ParseMultiplication();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseMultiplication()
    {
        var left = ParseExponentiation();
        while (Tokens.Match(SyntaxKind.Star) || Tokens.Match(SyntaxKind.Slash) || Tokens.Match(SyntaxKind.SlashSlash) || Tokens.Match(SyntaxKind.Percent))
        {
            var op = Tokens.Previous!;
            var right = ParseExponentiation();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseExponentiation()
    {
        var left = ParseUnary();
        while (Tokens.Match(SyntaxKind.Carat))
        {
            var op = Tokens.Previous!;
            var right = ParseUnary();
            left = new BinaryOp(left, op, right);
        }

        return left;
    }

    private Expression ParseUnary()
    {
        if (Tokens.Match(SyntaxKind.Bang) ||
            Tokens.Match(SyntaxKind.Tilde) ||
            Tokens.Match(SyntaxKind.PlusPlus) ||
            Tokens.Match(SyntaxKind.MinusMinus) ||
            Tokens.Match(SyntaxKind.Minus))
        {
            var op = Tokens.Previous!;
            var operand = ParseUnary(); // recursively parse the operand
            var isAssignmentOp = op.IsKind(SyntaxKind.PlusPlus) || op.IsKind(SyntaxKind.MinusMinus);
            if (isAssignmentOp && operand is not Name)
                _diagnostics.Error(DiagnosticCode.H006, $"Attempt to {(op.IsKind(SyntaxKind.PlusPlus) ? "in" : "de")}crement a constant, expected identifier", operand.GetFirstToken());

            return new UnaryOp(operand, op);
        }

        return ParsePostfix();
    }
    
    private Expression ParsePostfix()
    {
        var expression = ParsePrimary();

        while (true)
        {
            if (Tokens.Match(SyntaxKind.Dot))
                expression = ParseMemberAccess(expression);
            else if (Tokens.Match(SyntaxKind.Bang))
                expression = new PostfixOp(expression, Tokens.Previous!);
            else if (Tokens.Match(SyntaxKind.LBracket))
                expression = ParseElementAccess(expression);
            else if (IsInvocation())
                expression = ParseInvocation(expression);
            else
                break; // No more postfix operations
        }

        return expression;
    }

    private Expression ParsePrimary()
    {
        var token = Tokens.Advance();
        switch (token?.Kind)
        {
            case SyntaxKind.BoolLiteral:
            case SyntaxKind.CharLiteral:
            case SyntaxKind.StringLiteral:
            case SyntaxKind.IntLiteral:
            case SyntaxKind.FloatLiteral:
            case SyntaxKind.NoneKeyword:
                return new Literal(token);

            case SyntaxKind.Identifier:
                return new IdentifierName(token);

            case SyntaxKind.NameofKeyword:
            {
                Tokens.Consume(SyntaxKind.LParen);
                var identifier = Tokens.Consume(SyntaxKind.Identifier);
                if (identifier == null)
                    return new NoOp();
                    
                Tokens.Consume(SyntaxKind.RParen);

                return new NameOf(new IdentifierName(identifier));
            }

            case SyntaxKind.LParen:
            {
                var expression = ParseExpression();
                if (expression is NoOp)
                {
                    _diagnostics.Error(DiagnosticCode.H004D, $"Expected expression, got '{Tokens.Previous?.Kind.ToString() ?? "EOF"}'", Tokens.Previous!);
                    return new NoOp();
                }

                Tokens.Consume(SyntaxKind.RParen);
                return new Parenthesized(expression);
            }

            case SyntaxKind.LBrace:
            {
                var brace = token.WithKind(SyntaxKind.ObjectLiteral);
                return Tokens.Match(SyntaxKind.RBrace)
                    ? new ObjectLiteral(brace)
                    : ParseObject(brace);
            }

            case SyntaxKind.LBracket:
            {
                var bracket = token.WithKind(SyntaxKind.ArrayLiteral);
                return Tokens.Match(SyntaxKind.RBracket)
                    ? new ArrayLiteral(bracket)
                    : ParseArray(bracket);
            }
        }

        _diagnostics.Error(DiagnosticCode.H001B, $"Unexpected token '{token?.Kind}'", token ?? Tokens.Peek(-2));
        return new NoOp();
    }

    private bool IsAtEndOfBlock(int offset = 0) =>
        Tokens.Check(SyntaxKind.RBrace, offset) || Tokens.Peek(offset) == null;
        
    private void UnnecessaryParentheses(SyntaxNode node) =>
        _diagnostics.Warn(DiagnosticCode.H014, "Unnecessary parentheses", node);
}