using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement( ImmutableArray<BoundStatement> statements )
        {
            this.Statements = statements;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
        public ImmutableArray<BoundStatement> Statements { get; }
    }
}
