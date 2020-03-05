using System.Collections.Immutable;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class BlockStatementSyntax : StatementSyntax
    {
        public BlockStatementSyntax( SyntaxToken openBraceToken , ImmutableArray<StatementSyntax> statements , SyntaxToken closeBraceToken )
        {
            this.OpenBraceToken  = openBraceToken;
            this.Statements      = statements;
            this.CloseBraceToken = closeBraceToken;
        }

        public override SyntaxKind Kind => SyntaxKind.BlockStatement;
        public SyntaxToken OpenBraceToken { get; }
        public ImmutableArray<StatementSyntax> Statements { get; }
        public SyntaxToken CloseBraceToken { get; }
    }
}
