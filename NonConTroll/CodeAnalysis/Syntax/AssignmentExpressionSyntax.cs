namespace NonConTroll.CodeAnalysis.Syntax
{
    public class AssignmentExpressionSyntax : ExpressionSyntax
    {
        public AssignmentExpressionSyntax( SyntaxTree syntaxTree , SyntaxToken identifierToken , SyntaxToken equalsToken , ExpressionSyntax expression )
			: base( syntaxTree )
        {
            this.IdentifierToken = identifierToken;
            this.EqualsToken     = equalsToken;
            this.Expression      = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.AssignmentExpression;
        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Expression { get; }
    }
}
