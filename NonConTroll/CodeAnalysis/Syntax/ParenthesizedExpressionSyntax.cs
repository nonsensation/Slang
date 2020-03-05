namespace NonConTroll.CodeAnalysis.Syntax
{
    public class ParenthesizedExpressionSyntax : ExpressionSyntax
    {
        public ParenthesizedExpressionSyntax( SyntaxToken openParenthesisToken , ExpressionSyntax expression , SyntaxToken closeParenthesisToken )
        {
            this.OpenParenthesisToken  = openParenthesisToken ;
            this.Expression            = expression           ;
            this.CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;
        public SyntaxToken OpenParenthesisToken { get; }
        public ExpressionSyntax Expression { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }
}
