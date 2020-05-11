using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression( VariableSymbol variable )
        {
            this.Variable = variable;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public override TypeSymbol Type => this.Variable.Type;
        public VariableSymbol Variable { get; }
    }

}
