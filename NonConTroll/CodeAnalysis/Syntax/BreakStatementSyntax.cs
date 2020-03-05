namespace NonConTroll.CodeAnalysis.Syntax
{
    public class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax( SyntaxToken keyword )
        {
            this.Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        public SyntaxToken Keyword { get; }
    }
}
