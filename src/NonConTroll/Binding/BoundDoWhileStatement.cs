using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundDoWhileStatement : BoundLoopStatement
    {
        public BoundDoWhileStatement( BoundStatement body , BoundExpression condition , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Body = body;
            this.Condition = condition;
        }

        public override BoundNodeKind Kind => BoundNodeKind.DoWhileStatement;

        public BoundStatement Body { get; }
        public BoundExpression Condition { get; }
    }

}
