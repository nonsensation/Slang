using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol( string name , ImmutableArray<ParameterSymbol> parameters , TypeSymbol returnType , FunctionDeclarationSyntax? declaration = null )
            : base( name )
        {
            this.Parameters  = parameters;
            this.ReturnType  = returnType;
            this.Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public FunctionDeclarationSyntax? Declaration { get; }
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
    }
}
