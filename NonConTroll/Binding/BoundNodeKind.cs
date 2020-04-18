namespace NonConTroll.CodeAnalysis.Binding
{
    public enum BoundNodeKind
    {
        VariableDeclaration,
        TypeDeclaration,

        #region Statements
        BlockStatement,
        IfStatement,
        WhileStatement,
        DoWhileStatement,
        ForStatement,
        LabelStatement,
        GotoStatement,
        ConditionalGotoStatement,
        ReturnStatement,
        ExpressionStatement,
        DeferStatement,
        #endregion

        #region Expressions
        ErrorExpression,
        LiteralExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        BinaryExpression,
        CallExpression,
        ConversionExpression,
        #endregion
    }
}
