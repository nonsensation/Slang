using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundGlobalScope
    {
        public BoundGlobalScope( BoundGlobalScope? previous ,
                                 BuiltinFunctionSymbol? mainFunction ,
                                 BuiltinFunctionSymbol? evalFunction ,
                                 ImmutableArray<FunctionSymbol> functions ,
                                 ImmutableArray<VariableSymbol> variables ,
                                 ImmutableArray<BoundStatement> statements ,
                                 ImmutableArray<Diagnostic> diagnostics )
        {
            this.Previous     = previous;
            this.MainFunction = mainFunction;
            this.EvalFunction = evalFunction;
            this.Functions    = functions;
            this.Variables    = variables;
            this.Statements   = statements;
            this.Diagnostics  = diagnostics;
        }

        public BoundGlobalScope? Previous { get; }
        public BuiltinFunctionSymbol? MainFunction { get; }
        public BuiltinFunctionSymbol? EvalFunction { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }

}
