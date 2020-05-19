using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public BoundProgram( BoundProgram? previous ,
                             BuiltinFunctionSymbol? mainFunction ,
                             BuiltinFunctionSymbol? evalFunction ,
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
        public BuiltinFunctionSymbol? MainFunction { get; }
        public BuiltinFunctionSymbol? EvalFunction { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableDictionary<FunctionSymbol , BoundBlockStatement> Functions { get; }
    }
}
