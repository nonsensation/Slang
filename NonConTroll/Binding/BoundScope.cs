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

        public bool TryLookupVariable( string name , out VariableSymbol? variable )
            => this.TryLookupSymbol( name , out variable );

        public bool TryLookupFunction( string name , out FunctionSymbol? function )
            => this.TryLookupSymbol( name , out function );

        private bool TryLookupSymbol<TSymbol>( string name , out TSymbol? symbol )
            where TSymbol : Symbol
        {
            symbol = null;

            if( this.Symbols != null && this.Symbols.TryGetValue( name , out var declaredSymbol ) )
            {
                if( declaredSymbol is TSymbol matchingSymbol )
                {
                    symbol = matchingSymbol;

                    return true;
                }

                return false;
            }

            if( this.Parent == null )
                return false;

            return this.Parent.TryLookupSymbol( name , out symbol );
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
