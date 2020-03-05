namespace NonConTroll.CodeAnalysis.Syntax
{
    public class DoWhileStatementSyntax : StatementSyntax
    {
        public DoWhileStatementSyntax( SyntaxToken doKeyword , StatementSyntax body , SyntaxToken whileKeyword , ExpressionSyntax condition )
        {
            this.DoKeyword    = doKeyword;
            this.Body         = body;
            this.WhileKeyword = whileKeyword;
            this.Condition    = condition;
        }

        public override SyntaxKind Kind => SyntaxKind.DoWhileStatement;
        public SyntaxToken DoKeyword { get; }
        public StatementSyntax Body { get; }
        public SyntaxToken WhileKeyword { get; }
        public ExpressionSyntax Condition { get; }
    }
}
