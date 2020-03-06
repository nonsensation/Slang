namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundGotoStatement : BoundStatement
    {
        public BoundGotoStatement( BoundLabel label )
        {
            this.Label = label;
        }

        public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
        public BoundLabel Label { get; }
    }

}
