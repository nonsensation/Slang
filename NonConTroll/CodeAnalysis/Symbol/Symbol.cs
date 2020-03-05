using System.IO;

namespace NonConTroll.CodeAnalysis.Symbols
{
    public abstract class Symbol
    {
        private protected Symbol( string name )
        {
            this.Name = name;
        }

        public abstract SymbolKind Kind { get; }
        public string Name { get; }

        public void WriteTo( TextWriter writer )
        {
            SymbolPrinter.WriteTo( this , writer );
        }

        public override string ToString()
        {
            using var writer = new StringWriter();

            WriteTo( writer );

            return writer.ToString();
        }
    }
}
