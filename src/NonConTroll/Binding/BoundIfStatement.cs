using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement( BoundExpression condition , BoundStatement thenStatement , BoundStatement? elseStatement )
        {
            this.Condition = condition;
            this.ThenStatement = thenStatement;
            this.ElseStatement = elseStatement;
        }

        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;

        public BoundExpression Condition { get; }
        public BoundStatement ThenStatement { get; }
        public BoundStatement? ElseStatement { get; }
    }
}
