using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundVariableDeclaration : BoundStatement
    {
        public BoundVariableDeclaration( VariableSymbol variable , BoundExpression initializer )
        {
            this.Variable = variable;
            this.Initializer = initializer;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableDeclaration;
        public VariableSymbol Variable { get; }
        public BoundExpression Initializer { get; }
    }

}
