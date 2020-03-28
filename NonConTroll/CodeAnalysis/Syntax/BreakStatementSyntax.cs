namespace NonConTroll.CodeAnalysis.Syntax
{
    public class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword )
			: base( syntaxTree )
        {
            this.Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.BreakStatement;
        public SyntaxToken Keyword { get; }
    }
}
