using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class YieldStatementSyntax : StatementSyntax
    {
        public YieldStatementSyntax( SyntaxTree syntaxTree , SyntaxToken yieldKeyword , StatementSyntax expression )
			: base( syntaxTree )
        {
            this.YieldKeyword = yieldKeyword;
            this.Statement = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.YieldStatement;

        public SyntaxToken YieldKeyword { get; }
        public StatementSyntax Statement { get; }
    }
}

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundYieldStatement : BoundStatement
    {
        public BoundYieldStatement( BoundStatement expression )
        {
            this.Statement = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.YieldStatement;
        public BoundStatement Statement { get; }
    }
}
