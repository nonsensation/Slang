namespace NonConTroll.CodeAnalysis.Syntax
{
    public class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax left , SyntaxToken operatorToken , ExpressionSyntax right )
			: base( syntaxTree )
        {
            this.Lhs          = left         ;
            this.OperatorToken = operatorToken;
            this.Rhs         = right        ;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public ExpressionSyntax Lhs { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Rhs { get; }
    }
}
