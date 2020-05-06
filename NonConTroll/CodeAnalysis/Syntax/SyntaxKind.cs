namespace NonConTroll.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        None = 0,

        CompilationUnit,
        Parameter,
        TypeClause,

        VariableDeclaration,
        FunctionDeclaration,

        GlobalStatement,
        BlockStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        BreakStatement,
        ContinueStatement,
        ReturnStatement,
        DeferStatement,
        MatchExpression,
        ExpressionStatement,
        IfStatement,
        ElseClause,

        NameExpression,
        CallExpression,
        InfixCallExpression,
        UnaryExpression,
        BinaryExpression,
        AssignmentExpression,
        ParenthesizedExpression,
        LiteralExpression,
        TupleExpression,

        Type,
        TupleType,
        TupleElementType,
        TypeSpecifier,
        ArrayTypeSpecifier,

        Discard,
        Annotation,

        QualifiedName,
        IdentifierName,


        PatternExpression,

        MatchAnyPattern,
        ConstantPattern,
        InfixPattern,
    }
}
