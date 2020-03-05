namespace NonConTroll.CodeAnalysis.Syntax
{
    public class ElseClauseSyntax : SyntaxNode
    {
        public ElseClauseSyntax( SyntaxToken elseKeyword , StatementSyntax elseStatement )
        {
            this.ElseKeyword   = elseKeyword;
            this.ElseStatement = elseStatement;
        }

        public override SyntaxKind Kind => SyntaxKind.ElseClause;
        public SyntaxToken ElseKeyword { get; }
        public StatementSyntax ElseStatement { get; }
    }
}
