using System;
using System.IO;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.IO;

namespace NonConTroll.CodeAnalysis.Symbols
{
    internal static class SymbolPrinter
    {
        public static void WriteTo( Symbol symbol , TextWriter writer )
        {
            switch( symbol.Kind )
            {
                case SymbolKind.DeclaredFunction: WriteFunctionTo(       (DeclaredFunctionSymbol)symbol  , writer ); break;
                case SymbolKind.BuiltinFunction:  WriteFunctionTo(       (BuiltinFunctionSymbol)symbol   , writer ); break;
                case SymbolKind.GlobalVariable:   WriteGlobalVariableTo( (GlobalVariableSymbol)symbol    , writer ); break;
                case SymbolKind.LocalVariable:    WriteLocalVariableTo(  (LocalVariableSymbol)symbol     , writer ); break;
                case SymbolKind.Parameter:        WriteParameterTo(      (ParameterSymbol)symbol         , writer ); break;
                case SymbolKind.DeclaredType:     WriteTypeTo(           (DeclaredTypeSymbol)symbol      , writer ); break;
                case SymbolKind.BuiltinType:      WriteTypeTo(           (BuiltinTypeSymbol)symbol       , writer ); break;
                default:
                    throw new Exception( $"Unexpected symbol: {symbol.Kind}" );
            }
        }

        private static void WriteFunctionTo( FunctionSymbol symbol , TextWriter writer )
        {
            writer.WriteKeyword( SyntaxKind.FuncKeyword );
            writer.WriteSpace();
            writer.WriteIdentifier( symbol.Name );
            writer.WritePunctuation( SyntaxKind.OpenParenToken );

            for( var i = 0 ; i < symbol.Parameters.Length ; i++ )
            {
                if( i > 0 )
                {
                    writer.WritePunctuation( SyntaxKind.CommaToken );
                }

                writer.WriteSpace();
                symbol.Parameters[ i ].WriteTo( writer );
                writer.WriteSpace();
            }

            writer.WritePunctuation( SyntaxKind.CloseParenToken );
            writer.WriteSpace();
            writer.WritePunctuation( SyntaxKind.ColonToken );
            writer.WriteSpace();
            symbol.ReturnType.WriteTo( writer );
        }

        private static void WriteGlobalVariableTo( GlobalVariableSymbol symbol , TextWriter writer )
        {
            writer.WriteKeyword( symbol.IsReadOnly ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword );
            writer.WriteSpace();
            writer.WriteIdentifier( symbol.Name );
            writer.WritePunctuation( SyntaxKind.ColonToken );
            writer.WriteSpace();

            symbol.Type.WriteTo( writer );
        }

        private static void WriteLocalVariableTo( LocalVariableSymbol symbol , TextWriter writer )
        {
            writer.WriteKeyword( symbol.IsReadOnly ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword );
            writer.WriteSpace();
            writer.WriteIdentifier( symbol.Name );
            writer.WritePunctuation( SyntaxKind.ColonToken );
            writer.WriteSpace();

            symbol.Type.WriteTo( writer );
        }

        private static void WriteParameterTo( ParameterSymbol symbol , TextWriter writer )
        {
            writer.WriteIdentifier( symbol.Name );
            writer.WritePunctuation( SyntaxKind.ColonToken );
            writer.WriteSpace();

            symbol.Type.WriteTo( writer );
        }

        private static void WriteTypeTo( TypeSymbol symbol , TextWriter writer )
        {
            writer.WriteIdentifier( symbol.Name );
        }
    }
}
