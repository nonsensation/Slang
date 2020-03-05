namespace NonConTroll.CodeAnalysis.Syntax
{
    public class CallExpressionSyntax : ExpressionSyntax
    {
        public CallExpressionSyntax( SyntaxToken identifier , SyntaxToken openParenthesisToken , SeparatedSyntaxList<ExpressionSyntax> arguments , SyntaxToken closeParenthesisToken )
        {
            this.Identifier            = identifier           ;
            this.OpenParenthesisToken  = openParenthesisToken ;
            this.Arguments             = arguments            ;
            this.CloseParenthesisToken = closeParenthesisToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CallExpression;
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenthesisToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParenthesisToken { get; }
    }
}
