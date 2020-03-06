using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundUnaryExpression : BoundExpression
    {
        public BoundUnaryExpression( BoundUnaryOperator op , BoundExpression operand )
        {
            this.Op = op;
            this.Operand = operand;
        }

        public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
        public override TypeSymbol Type => this.Op.Type;
        public BoundUnaryOperator Op { get; }
        public BoundExpression Operand { get; }
    }

}
