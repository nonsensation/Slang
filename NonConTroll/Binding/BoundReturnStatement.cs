using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundReturnStatement : BoundStatement
    {
        public BoundReturnStatement( BoundExpression? expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;

        public BoundExpression? Expression { get; }
    }

}
