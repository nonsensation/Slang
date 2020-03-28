namespace NonConTroll.CodeAnalysis.Syntax
{
    public class LiteralExpressionSyntax : ExpressionSyntax
    {
        public LiteralExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken literalToken )
			: base( syntaxTree )
        {
            this.LiteralToken = literalToken;
        }

        public override SyntaxKind Kind => SyntaxKind.LiteralExpression;
        public SyntaxToken LiteralToken { get; }
    }
}
