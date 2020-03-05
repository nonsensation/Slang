namespace NonConTroll.CodeAnalysis.Syntax
{
    public class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax( SyntaxToken whileKeyword , ExpressionSyntax condition , StatementSyntax body )
        {
            this.WhileKeyword = whileKeyword;
            this.Condition    = condition;
            this.Body         = body;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }
    }
}
