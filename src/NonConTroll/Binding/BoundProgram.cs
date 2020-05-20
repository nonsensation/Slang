using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public BoundProgram( BoundProgram? previous ,
                             FunctionSymbol? mainFunction ,
                             FunctionSymbol? evalFunction ,
                             ImmutableArray<Diagnostic> diagnostics ,
                             ImmutableDictionary<FunctionSymbol , BoundBlockStatement> functions )
        {
            this.Previous     = previous;
            this.MainFunction = mainFunction;
            this.EvalFunction = evalFunction;
            this.Diagnostics  = diagnostics;
            this.Functions    = functions;
        }

        public BoundProgram? Previous { get; }
        public FunctionSymbol? MainFunction { get; }
        public FunctionSymbol? EvalFunction { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableDictionary<FunctionSymbol , BoundBlockStatement> Functions { get; }
    }
}
