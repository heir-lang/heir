﻿using Heir.CodeGeneration;
using Heir.Syntax;

namespace Heir.AST
{
    public class Block(List<SyntaxNode> statements) : Statement
    {
        public List<SyntaxNode> Statements { get; } = statements;

        public override void Display(int indent = 0)
        {
            foreach (var statement in Statements)
                statement.Display(indent);
        }

        public List<Instruction> GenerateBytecode() => [];
            //Statements
            //.Select(statement => statement.GenerateBytecode())
            //.Aggregate((finalBytecode, statementBytecode) => finalBytecode.Concat(statementBytecode).ToList());

        public override List<Token> GetTokens() =>
            Statements
            .Select(statement => statement.GetTokens())
            .Aggregate((allTokens, statementTokens) => allTokens.Concat(statementTokens).ToList());
    }
}
