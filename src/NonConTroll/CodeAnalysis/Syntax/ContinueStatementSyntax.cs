namespace NonConTroll.CodeAnalysis.Syntax
{
    public class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword )
			: base( syntaxTree )
        {
            this.Keyword = keyword;
        }

        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;
        public SyntaxToken Keyword { get; }
    }
}
