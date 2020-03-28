namespace NonConTroll.CodeAnalysis.Syntax
{
    public class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax( SyntaxTree syntaxTree , SyntaxToken returnKeyword , ExpressionSyntax? expression )
			: base( syntaxTree )
		{
            this.ReturnKeyword = returnKeyword;
            this.Expression    = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;
        public SyntaxToken ReturnKeyword { get; }
        public ExpressionSyntax? Expression { get; }
    }
}
