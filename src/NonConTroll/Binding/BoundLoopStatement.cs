namespace NonConTroll.CodeAnalysis.Binding
{
    public abstract class BoundLoopStatement : BoundStatement
    {
        protected BoundLoopStatement( BoundLabel breakLabel , BoundLabel continueLabel )
        {
            this.BreakLabel = breakLabel;
            this.ContinueLabel = continueLabel;
        }

        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}
