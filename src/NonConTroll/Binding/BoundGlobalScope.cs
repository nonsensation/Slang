using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundGlobalScope
    {
        public BoundGlobalScope( BoundGlobalScope? previous , ImmutableArray<Diagnostic> diagnostics , ImmutableArray<FunctionSymbol> functions , ImmutableArray<VariableSymbol> variables , ImmutableArray<BoundStatement> statements )
        {
            this.Previous = previous;
            this.Diagnostics = diagnostics;
            this.Functions = functions;
            this.Variables = variables;
            this.Statements = statements;
        }

        public BoundGlobalScope? Previous { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableArray<FunctionSymbol> Functions { get; }
        public ImmutableArray<VariableSymbol> Variables { get; }
        public ImmutableArray<BoundStatement> Statements { get; }
    }

}
