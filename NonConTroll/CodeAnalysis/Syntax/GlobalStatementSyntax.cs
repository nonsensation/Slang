namespace NonConTroll.CodeAnalysis.Syntax
{
    public class GlobalStatementSyntax : MemberSyntax
    {
        public GlobalStatementSyntax( SyntaxTree syntaxTree , StatementSyntax statement )
			: base( syntaxTree )
        {
            this.Statement = statement;
        }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;
        public StatementSyntax Statement { get; }
    }
}
