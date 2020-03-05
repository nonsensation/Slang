namespace NonConTroll.CodeAnalysis.Syntax
{
    public class VariableDeclarationSyntax : StatementSyntax
    {
        public VariableDeclarationSyntax( SyntaxToken keyword , SyntaxToken identifier , TypeClauseSyntax? typeClause , SyntaxToken equalsToken , ExpressionSyntax initializer )
        {
            this.Keyword = keyword;
            this.Identifier = identifier;
            this.TypeClause = typeClause;
            this.EqualsToken = equalsToken;
            this.Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;
        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax? TypeClause { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }
    }
}
