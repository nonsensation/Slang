using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundForStatement : BoundLoopStatement
    {
        public BoundForStatement( VariableSymbol variable , BoundExpression lowerBound , BoundExpression upperBound , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Variable = variable;
            this.LowerBound = lowerBound;
            this.UpperBound = upperBound;
            this.Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
        public VariableSymbol Variable { get; }
        public BoundExpression LowerBound { get; }
        public BoundExpression UpperBound { get; }
        public BoundStatement Body { get; }
    }

}
