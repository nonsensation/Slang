namespace NonConTroll.CodeAnalysis.Syntax
{
	public class UnaryExpressionSyntax : ExpressionSyntax
	{
		public UnaryExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken operatorToken , ExpressionSyntax operand )
			: base( syntaxTree )
		{
			this.OperatorToken = operatorToken;
			this.Operand       = operand;
		}

		public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
		public SyntaxToken OperatorToken { get; }
		public ExpressionSyntax Operand { get; }
	}


}
