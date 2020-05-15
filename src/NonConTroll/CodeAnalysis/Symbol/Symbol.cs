using System.IO;

namespace NonConTroll.CodeAnalysis.Symbols
{
    public enum SymbolKind
    {
        BuiltinFunction,
        DeclaredFunction,
        GlobalVariable,
        LocalVariable,
        Parameter,
        DeclaredType,
        BuiltinType,
    }

    public abstract class Symbol
    {
        private protected Symbol( string name )
        {
            this.Name = name;
        }

        public abstract SymbolKind Kind { get; }
        public bool IsResolved { get; set; } = false;
        public string Name { get; }

        public void WriteTo( TextWriter writer )
        {
            SymbolPrinter.WriteTo( this , writer );
        }

        public override string ToString()
        {
            using var writer = new StringWriter();

			this.WriteTo( writer );

            return writer.ToString();
        }
    }
}
