using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundCallExpression : BoundExpression
    {
        public BoundCallExpression( FunctionSymbol function , ImmutableArray<BoundExpression> arguments )
        {
            this.Function = function;
            this.Arguments = arguments;
        }

        public override BoundNodeKind Kind => BoundNodeKind.CallExpression;
        public override TypeSymbol Type => this.Function.ReturnType;
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }

}
