using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Symbols;

namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundProgram
    {
        public BoundProgram( ImmutableArray<Diagnostic> diagnostics , ImmutableDictionary<FunctionSymbol , BoundBlockStatement> functions , BoundBlockStatement statement )
        {
            this.Diagnostics = diagnostics;
            this.Functions = functions;
            this.Statement = statement;
        }

        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public ImmutableDictionary<FunctionSymbol , BoundBlockStatement> Functions { get; }
        public BoundBlockStatement Statement { get; }
    }

}
