namespace NonConTroll.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly TypeSymbol Error = new TypeSymbol( "?" );

        public static readonly TypeSymbol Int = new TypeSymbol( "int" );
        public static readonly TypeSymbol String = new TypeSymbol( "string" );
        public static readonly TypeSymbol Void = new TypeSymbol( "void" );

        public static readonly TypeSymbol I8 = new TypeSymbol( "i8" );
        public static readonly TypeSymbol U8 = new TypeSymbol( "u8" );
        public static readonly TypeSymbol I16 = new TypeSymbol( "i16" );
        public static readonly TypeSymbol U16 = new TypeSymbol( "u16" );
        public static readonly TypeSymbol I32 = new TypeSymbol( "i32" );
        public static readonly TypeSymbol U32 = new TypeSymbol( "u32" );
        public static readonly TypeSymbol I64 = new TypeSymbol( "i64" );
        public static readonly TypeSymbol U64 = new TypeSymbol( "u64" );
        public static readonly TypeSymbol F32 = new TypeSymbol( "f32" );
        public static readonly TypeSymbol F64 = new TypeSymbol( "f64" );

        public static readonly TypeSymbol Byte = new TypeSymbol( "byte" );
        public static readonly TypeSymbol Bool = new TypeSymbol( "bool" );
        public static readonly TypeSymbol Nint = new TypeSymbol( "nint" );
        public static readonly TypeSymbol Nuint = new TypeSymbol( "nuint" );
        public static readonly TypeSymbol Char = new TypeSymbol( "char" );
        public static readonly TypeSymbol Rune = new TypeSymbol( "rune" );

        private TypeSymbol( string name )
            : base( name )
        {
        }

        public override SymbolKind Kind => SymbolKind.Type;
    }
}
