using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace NonConTroll.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly BuiltinFunctionSymbol Print
            = new BuiltinFunctionSymbol( "print" ,
                                         ImmutableArray.Create( new ParameterSymbol( "text" , BuiltinTypes.String ) ) ,
                                         BuiltinTypes.Void , null );
        public static readonly BuiltinFunctionSymbol Input
            = new BuiltinFunctionSymbol( "input" ,
                                         ImmutableArray<ParameterSymbol>.Empty ,
                                         BuiltinTypes.String , null );
        public static readonly BuiltinFunctionSymbol Rnd
            = new BuiltinFunctionSymbol( "rnd" ,
                                         ImmutableArray.Create( new ParameterSymbol( "max" , BuiltinTypes.Int ) ) ,
                                         BuiltinTypes.Int , null );

        public static IEnumerable<BuiltinFunctionSymbol> GetAll()
            => typeof( BuiltinFunctions )
                .GetFields( BindingFlags.Public | BindingFlags.Static )
                .Where( f => f.FieldType == typeof( BuiltinFunctionSymbol ) )
                .Select( f => f.GetValue( null ) )
                .Where( f => f != null )
                .Cast<BuiltinFunctionSymbol>();
    }

    internal static class BuiltinTypes
    {
        public static readonly BuiltinTypeSymbol Error = new BuiltinTypeSymbol( "?" );

        // implemented
        public static readonly BuiltinTypeSymbol Int = new BuiltinTypeSymbol( "int" );
        public static readonly BuiltinTypeSymbol String = new BuiltinTypeSymbol( "string" );
        public static readonly BuiltinTypeSymbol Void = new BuiltinTypeSymbol( "void" );
        public static readonly BuiltinTypeSymbol Any = new BuiltinTypeSymbol( "any" );
        public static readonly BuiltinTypeSymbol Bool = new BuiltinTypeSymbol( "bool" );

        // not implemented
        public static readonly BuiltinTypeSymbol I8 = new BuiltinTypeSymbol( "i8" );
        public static readonly BuiltinTypeSymbol U8 = new BuiltinTypeSymbol( "u8" );
        public static readonly BuiltinTypeSymbol I16 = new BuiltinTypeSymbol( "i16" );
        public static readonly BuiltinTypeSymbol U16 = new BuiltinTypeSymbol( "u16" );
        public static readonly BuiltinTypeSymbol I32 = new BuiltinTypeSymbol( "i32" );
        public static readonly BuiltinTypeSymbol U32 = new BuiltinTypeSymbol( "u32" );
        public static readonly BuiltinTypeSymbol I64 = new BuiltinTypeSymbol( "i64" );
        public static readonly BuiltinTypeSymbol U64 = new BuiltinTypeSymbol( "u64" );
        public static readonly BuiltinTypeSymbol F32 = new BuiltinTypeSymbol( "f32" );
        public static readonly BuiltinTypeSymbol F64 = new BuiltinTypeSymbol( "f64" );

        public static readonly BuiltinTypeSymbol Byte = new BuiltinTypeSymbol( "byte" );
        public static readonly BuiltinTypeSymbol Nint = new BuiltinTypeSymbol( "nint" );
        public static readonly BuiltinTypeSymbol Nuint = new BuiltinTypeSymbol( "nuint" );
        public static readonly BuiltinTypeSymbol Char = new BuiltinTypeSymbol( "char" );
        public static readonly BuiltinTypeSymbol Rune = new BuiltinTypeSymbol( "rune" );

        public static IEnumerable<BuiltinTypeSymbol> GetAll()
            => typeof( BuiltinTypeSymbol )
                .GetFields( BindingFlags.Public | BindingFlags.Static )
                .Where( f => f.FieldType == typeof( BuiltinTypeSymbol ) )
                .Where( f => f.Name != "Error" )
                .Select( f => f.GetValue( null ) )
                .Where( f => f != null )
                .Cast<BuiltinTypeSymbol>();
    }
}
