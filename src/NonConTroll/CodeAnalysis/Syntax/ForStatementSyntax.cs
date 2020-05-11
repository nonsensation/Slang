namespace NonConTroll.CodeAnalysis.Syntax
{
    public class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax( SyntaxTree syntaxTree , SyntaxToken keyword , SyntaxToken identifier , SyntaxToken equalsToken , ExpressionSyntax lowerBound , SyntaxToken toKeyword , ExpressionSyntax upperBound , StatementSyntax body )
			: base( syntaxTree )
        {
            this.Keyword     = keyword;
            this.Identifier  = identifier;
            this.EqualsToken = equalsToken;
            this.LowerBound  = lowerBound;
            this.ToKeyword   = toKeyword;
            this.UpperBound  = upperBound;
            this.Body        = body;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax LowerBound { get; }
        public SyntaxToken ToKeyword { get; }
        public ExpressionSyntax UpperBound { get; }
        public StatementSyntax Body { get; }
    }
}
