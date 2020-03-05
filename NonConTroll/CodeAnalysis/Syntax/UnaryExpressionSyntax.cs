namespace NonConTroll.CodeAnalysis.Syntax
{
    public class UnaryExpressionSyntax : ExpressionSyntax
    {
        public UnaryExpressionSyntax( SyntaxToken operatorToken , ExpressionSyntax operand )
        {
            this.OperatorToken = operatorToken;
            this.Operand       = operand      ;
        }

        public override SyntaxKind Kind => SyntaxKind.UnaryExpression;
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Operand { get; }
    }


}
