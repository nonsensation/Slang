namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundLabelStatement : BoundStatement
    {
        public BoundLabelStatement( BoundLabel label )
        {
            this.Label = label;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
        public BoundLabel Label { get; }
    }

}
