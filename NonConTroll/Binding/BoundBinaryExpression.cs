using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundBinaryExpression : BoundExpression
    {
        public BoundBinaryExpression( BoundExpression lhs , BoundBinaryOperator op , BoundExpression rhs )
        {
            this.Lhs = lhs;
            this.Op = op;
            this.Rhs = rhs;
        }

        public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
        public override TypeSymbol Type => this.Op.Type;
        public BoundExpression Lhs { get; }
        public BoundBinaryOperator Op { get; }
        public BoundExpression Rhs { get; }
    }

}
