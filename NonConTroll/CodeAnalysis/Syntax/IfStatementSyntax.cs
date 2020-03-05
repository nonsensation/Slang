namespace NonConTroll.CodeAnalysis.Syntax
{
    public class IfStatementSyntax : StatementSyntax
    {
        public IfStatementSyntax( SyntaxToken ifKeyword , ExpressionSyntax condition , StatementSyntax thenStatement , ElseClauseSyntax? elseClause )
        {
            this.IfKeyword     = ifKeyword;
            this.Condition     = condition;
            this.ThenStatement = thenStatement;
            this.ElseClause    = elseClause;
        }

        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public SyntaxToken IfKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax? ElseClause { get; }
    }
}
