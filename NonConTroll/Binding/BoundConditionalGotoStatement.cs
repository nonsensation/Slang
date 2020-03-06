namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundConditionalGotoStatement : BoundStatement
    {
        public BoundConditionalGotoStatement( BoundLabel label , BoundExpression condition , bool jumpIfTrue = true )
        {
            this.Label = label;
            this.Condition = condition;
            this.JumpIfTrue = jumpIfTrue;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
        public BoundLabel Label { get; }
        public BoundExpression Condition { get; }
        public bool JumpIfTrue { get; }
    }

}
