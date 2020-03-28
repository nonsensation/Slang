using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NonConTroll.CodeAnalysis.Symbols;


namespace NonConTroll.CodeAnalysis.Binding
{
    public class BoundScope
    {
        private Dictionary<string, Symbol>? Symbols;

        public BoundScope( BoundScope? parent )
        {
            this.Parent = parent;
        }

        public BoundScope? Parent { get; }

        public bool TryDeclareVariable( VariableSymbol variable )
            => this.TryDeclareSymbol( variable );

        public bool TryDeclareFunction( FunctionSymbol function )
            => this.TryDeclareSymbol( function );

        private bool TryDeclareSymbol<TSymbol>( TSymbol symbol )
            where TSymbol : Symbol
        {
            if( this.Symbols == null )
                this.Symbols = new Dictionary<string , Symbol>();
            else if( this.Symbols.ContainsKey( symbol.Name ) )
                return false;

            this.Symbols.Add( symbol.Name , symbol );

            return true;
        }

        public Symbol? TryLookupSymbol( string name )
        {
            if( this.Symbols != null && this.Symbols.TryGetValue( name , out var symbol ) )
                return symbol;

            return this.Parent?.TryLookupSymbol( name );
        }

        public ImmutableArray<VariableSymbol> GetDeclaredVariables()
            => this.GetDeclaredSymbols<VariableSymbol>();

        public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
            => this.GetDeclaredSymbols<FunctionSymbol>();

        private ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>()
            where TSymbol : Symbol
        {
            if( this.Symbols == null )
                return ImmutableArray<TSymbol>.Empty;

            return this.Symbols.Values.OfType<TSymbol>().ToImmutableArray();
        }
    }

}
