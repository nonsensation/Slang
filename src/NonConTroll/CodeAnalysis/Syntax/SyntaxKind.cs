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
        ClassKeyword,
        StructKeyword,
        MixinKeyword,
        AbstractKeyword,
        MetaKeyword,
        TemplateKeyword,
        ImportKeyword,
        ExportKeyword,
        ImplicitKeyword,
        ExplicitKeyword,
        InternalKeyword,
        ExternalKeyword,
        PublicKeyword,
        PubKeyword,
        ProtectedKeyword,
        PrivateKeyword,
        IfKeyword,
        ThenKeyword,
        WhenKeyword,
        ElseKeyword,
        ElifKeyword,
        WhileKeyword,
        LoopKeyword,
        ForKeyword,
        ForeachKeyword,
        MatchKeyword,
        SwitchKeyword,
        CaseKeyword,
        DefaultKeyword,
        DeferKeyword,
        DeleteKeyword,
        ExceptKeyword,
        TryKeyword,
        CatchKeyword,
        ExpectKeyword,
        EnsureKeyword,
        InKeyword,
        OutKeyword,
        RefKeyword,
        ValKeyword,
        PtrKeyword,
        VarKeyword,
        LetKeyword,
        ConstKeyword,
        ReadonlyKeyword,
        VolatileKeyword,
        EnumKeyword,
        UnionKeyword,
        IsKeyword,
        AsKeyword,
        CastKeyword,
        OperatorKeyword,
        NamespaceKeyword,
        PackageKeyword,
        ModuleKeyword,
        BreakKeyword,
        ContinueKeyword,
        DoKeyword,
        ToKeyword,

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

        // SingleLineCommentTrivia,
        // MultiLineCommentTrivia,
        // SingleLineDocCommentTrivia,
        // MultiLineDocCommentTrivia,

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
        TypeDeclaration,

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

        PartialTypeSpecifier,
        NullableTypeSpecifier,
        ReferenceTypeSpecifier,
        PointerTypeSpecifier,
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
