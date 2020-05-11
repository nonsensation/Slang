using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundConversionExpression : BoundExpression
    {
        public BoundConversionExpression( TypeSymbol type , BoundExpression expression )
        {
            this.Type = type;
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; }
    }

}
