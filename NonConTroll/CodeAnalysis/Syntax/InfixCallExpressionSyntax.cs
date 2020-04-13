namespace NonConTroll.CodeAnalysis.Syntax
{
    public class InfixCallExpressionSyntax : ExpressionSyntax
    {
        public InfixCallExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax lhsExpression , SyntaxToken identifier , ExpressionSyntax rhsExpression )
			: base( syntaxTree )
		{
            this.Lhs        = lhsExpression;
            this.Identifier = identifier;
            this.Rhs        = rhsExpression;
        }

        public override SyntaxKind Kind => SyntaxKind.InfixCallExpression;
        public ExpressionSyntax Lhs { get; }
        public SyntaxToken Identifier { get; }
        public ExpressionSyntax Rhs { get; }
    }
}
