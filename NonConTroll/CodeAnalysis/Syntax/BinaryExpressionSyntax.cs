namespace NonConTroll.CodeAnalysis.Syntax
{
    public class BinaryExpressionSyntax : ExpressionSyntax
    {
        public BinaryExpressionSyntax( SyntaxTree syntaxTree , ExpressionSyntax left , SyntaxToken operatorToken , ExpressionSyntax right )
			: base( syntaxTree )
        {
            this.Left          = left         ;
            this.OperatorToken = operatorToken;
            this.Right         = right        ;
        }

        public override SyntaxKind Kind => SyntaxKind.BinaryExpression;
        public ExpressionSyntax Left { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Right { get; }
    }
}
