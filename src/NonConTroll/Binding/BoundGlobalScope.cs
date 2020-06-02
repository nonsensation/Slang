using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Binding
{
    internal sealed class BoundGlobalScope
    {
        public BoundGlobalScope( BoundGlobalScope? previous ,
                                 DeclaredFunctionSymbol? mainFunction ,
                                 DeclaredFunctionSymbol? evalFunction ,
                                 ImmutableArray<DeclaredFunctionSymbol> functions ,
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
        public DeclaredFunctionSymbol? MainFunction { get; }
        public DeclaredFunctionSymbol? EvalFunction { get; }
        public ImmutableArray<DeclaredFunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
    }

}
