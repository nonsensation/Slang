using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundDeferStatement : BoundStatement
    {
        public BoundDeferStatement( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DeferStatement;

        public BoundExpression Expression { get; }
    }
}
