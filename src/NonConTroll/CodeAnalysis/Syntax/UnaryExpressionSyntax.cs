namespace NonConTroll.CodeAnalysis.Syntax
{
	public class UnaryExpressionSyntax : ExpressionSyntax
	{
		public UnaryExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken operatorToken , ExpressionSyntax expression )
			: base( syntaxTree )
		{
			this.OperatorToken = operatorToken;
			this.Expression       = expression;
		}

		public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
		public SyntaxToken OperatorToken { get; }
		public ExpressionSyntax Expression { get; }
	}


}
