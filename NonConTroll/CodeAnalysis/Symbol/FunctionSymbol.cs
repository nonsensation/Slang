using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Symbols
{
    public class FunctionSymbol : Symbol
    {
        public FunctionSymbol( string name , ImmutableArray<ParameterSymbol> parameters , TypeSymbol returnType , FunctionDeclarationSyntax declaration )
            : base( name )
        {
            this.Parameters  = parameters;
            this.ReturnType  = returnType;
            this.Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public FunctionDeclarationSyntax Declaration { get; }
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
    }

    public sealed class BuiltinFunctionSymbol : Symbol
    {
        public BuiltinFunctionSymbol( string name , ImmutableArray<ParameterSymbol> parameters , TypeSymbol returnType )
            : base( name )
        {
            this.Parameters  = parameters;
            this.ReturnType  = returnType;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol ReturnType { get; }
    }
}
