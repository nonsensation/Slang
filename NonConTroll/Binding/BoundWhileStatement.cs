namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundWhileStatement : BoundLoopStatement
    {
        public BoundWhileStatement( BoundExpression condition , BoundStatement body , BoundLabel breakLabel , BoundLabel continueLabel )
            : base( breakLabel , continueLabel )
        {
            this.Condition = condition;
            this.Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition { get; }
        public BoundStatement Body { get; }
    }
}
