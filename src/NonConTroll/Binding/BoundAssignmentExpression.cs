using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundAssignmentExpression : BoundExpression
    {
        public BoundAssignmentExpression( VariableSymbol variable , BoundExpression expression )
        {
            this.Variable = variable;
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
        public override TypeSymbol Type => this.Expression.Type;
        public VariableSymbol Variable { get; }
        public BoundExpression Expression { get; }
    }

}
