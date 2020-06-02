using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Binding
{
    internal sealed class BoundProgram
    {
        public BoundProgram( BoundProgram? previous ,
                             FunctionSymbol? mainFunction ,
                             FunctionSymbol? evalFunction ,
                             ImmutableArray<Diagnostic> diagnostics ,
                             ImmutableDictionary<DeclaredFunctionSymbol , BoundBlockStatement> functions )
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
        public ImmutableDictionary<DeclaredFunctionSymbol , BoundBlockStatement> Functions { get; }
    }
}
