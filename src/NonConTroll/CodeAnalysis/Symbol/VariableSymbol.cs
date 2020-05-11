namespace NonConTroll.CodeAnalysis.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        internal VariableSymbol( string name , bool isReadOnly , TypeSymbol type )
            : base( name )
        {
            this.IsReadOnly = isReadOnly;
            this.Type = type;
        }

        public bool IsReadOnly { get; }
        public TypeSymbol Type { get; }
    }
}
