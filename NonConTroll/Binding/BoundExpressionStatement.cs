namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement( BoundExpression expression )
        {
            this.Expression = expression;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
        public BoundExpression Expression { get; }
    }

}
