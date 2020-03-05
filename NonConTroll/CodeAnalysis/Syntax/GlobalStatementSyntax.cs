namespace NonConTroll.CodeAnalysis.Syntax
{
    public class GlobalStatementSyntax : MemberSyntax
    {
        public GlobalStatementSyntax( StatementSyntax statement )
        {
            this.Statement = statement;
        }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
        public StatementSyntax Statement { get; }
    }
}
