namespace NonConTroll.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        None = 0,

        EndOfFile,

        Identifier,

        #region Keywords

        FuncKeyword,
        FnKeyword,
        ReturnKeyword,
        RetKeyword,
        Class,
        Struct,
        Mixin,
        Abstract,
        Meta,
        Template,
        Import,
        Export,
        Implicit,
        Explicit,
        Internal,
        External,
        Public,
        Pub,
        Protected,
        Private,
        IfKeyword,
        Then,
        When,
        ElseKeyword,
        Elif,
        WhileKeyword,
        Loop,
        ForKeyword,
        Foreach,
        MatchKeyword,
        Switch,
        Case,
        Default,
        DeferKeyword,
        Delete,
        Except,
        Try,
        Catch,
        Expect,
        Ensure,
        In,
        Out,
        RefKeyword,
        Val,
        PtrKeyword,
        VarKeyword,
        LetKeyword,
        Const,
        Readonly,
        Volatile,
        Enum,
        Union,
        Is,
        As,
        Cast,
        Operator,
        Namespace,
        Package,
        Module,
        BreakKeyword,
        ContinueKeyword,
        DoKeyword,
        To,
        YieldKeyword,

        TrueKeywordLiteral,
        FalseKeywordLiteral,
        NullKeywordLiteral,
        UndefinedKeywordLiteral,

        #endregion

        #region Punctuation

        DotToken,
        DotDotToken,
        DotDotDotToken,
        ColonToken,
        CommaToken,
        SemicolonToken,
        OpenParenToken,
        CloseParenToken,
        OpenBraceToken,
        CloseBraceToken,
        OpenBracketToken,
        CloseBracketToken,
        MinusToken,
        PlusToken,
        StarToken,
        SlashToken,
        EqToken,
        EqEqToken,
        ExmToken,
        QmToken,
        ExmEqToken,
        PipeToken,
        PipePipeToken,
        AndToken,
        AndAndToken,
        LtToken,
        LtEqToken,
        GtToken,
        GtEqToken,
        MinusGtToken,
        LtMinusToken,
        EqGtToken,
        PlusEq,
        MinusEqToken,
        StarEqToken,
        SlashEqToken,
        SingleQuoteToken,
        DoubleQuoteToken,
        PercentToken,
        DollarToken,
        CaretToken,
        TildeToken,
        HashtagToken,
        UnderscoreToken,
        BackSlash,
        BackTickToken,
        FrontTickToken,
        AtToken,

        #endregion

        #region Trivia

        WhiteSpaceTrivia,
        NewLineWhiteSpaceTrivia,
        IndentationWhiteSpaceTrivia,
        AlignmentWhiteSpaceTrivia,

        CommentTrivia,
        DocCommentTrivia,

        #endregion

        #region Literals

        StringLiteral,
        BooleanLiteral,
        NumericLiteral,
        DecimalLiteral,
        IntegerLiteral,
        CharacterLiteral,

        #endregion

        #region Parsed Syntax

        CompilationUnit,
        Parameter,
        TypeClause,
        Discard,
        Annotation,
        // declaration
        VariableDeclaration,
        FunctionDeclaration,
        // statements
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
        PatternSectionStatement,
        YieldStatement,

        // expressions
        NameExpression,
        CallExpression,
        InfixUnaryCallExpression,
        InfixBinaryCallExpression,
        UnaryExpression,
        BinaryExpression,
        AssignmentExpression,
        ParenthesizedExpression,
        LiteralExpression,
        TupleExpression,
        PatternSectionExpression,

        // types & names
        Type,
        TupleType,
        TupleElementType,
        TypeSpecifier,
        ArrayTypeSpecifier,
        QualifiedName,
        IdentifierName,

        // pattern matching
        MatchAnyPattern,
        ConstantPattern,
        InfixPattern,
        MatchStatement,

        #endregion
    }
}
