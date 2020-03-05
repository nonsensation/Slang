namespace NonConTroll.CodeAnalysis.Syntax
{
    public class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax( SyntaxToken keyword )
        {
            this.Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
        public SyntaxToken Keyword { get; }
    }
}
