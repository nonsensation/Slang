using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.IO
{
    public static class TextWriterExtensions
    {
        private static bool IsConsoleOut( this TextWriter writer )
        {
            if( writer == Console.Out )
                return true;

            if( writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut() )
                return true;

            return false;
        }

        private static void SetForeground( this TextWriter writer , ConsoleColor color )
        {
            if( writer.IsConsoleOut() )
                Console.ForegroundColor = color;
        }

        private static void ResetColor( this TextWriter writer )
        {
            if( writer.IsConsoleOut() )
                Console.ResetColor();
        }

        public static void WriteKeyword( this TextWriter writer , TokenType tokenType )
        {
            writer.WriteKeyword( tokenType.GetName() );
        }

        public static void WriteKeyword( this TextWriter writer , string? text )
        {
            writer.SetForeground( ConsoleColor.Blue );
            writer.Write( text );
            writer.ResetColor();
        }

        public static void WriteIdentifier( this TextWriter writer , string text )
        {
            writer.SetForeground( ConsoleColor.DarkYellow );
            writer.Write( text );
            writer.ResetColor();
        }

        public static void WriteNumber( this TextWriter writer , string text )
        {
            writer.SetForeground( ConsoleColor.Cyan );
            writer.Write( text );
            writer.ResetColor();
        }

        public static void WriteString( this TextWriter writer , string text )
        {
            writer.SetForeground( ConsoleColor.Magenta );
            writer.Write( text );
            writer.ResetColor();
        }

        public static void WriteSpace( this TextWriter writer )
        {
            writer.WritePunctuation( " " );
        }

        public static void WritePunctuation( this TextWriter writer , TokenType tokenType )
        {
            writer.WritePunctuation( tokenType.GetName() );
        }

        public static void WritePunctuation( this TextWriter writer , string? text )
        {
            writer.SetForeground( ConsoleColor.DarkGray );
            writer.Write( text );
            writer.ResetColor();
        }

        public static void WriteDiagnostics( this TextWriter writer , IEnumerable<Diagnostic> diagnostics , SyntaxTree syntaxTree )
        {
            foreach( var diagnostic in diagnostics.OrderBy( diag => diag.Span.Start )
                                                  .ThenBy( diag => diag.Span.Length ) )
            {
                var lineIndex  = syntaxTree.Text.GetLineIndex( diagnostic.Span.Start );
                var line       = syntaxTree.Text.Lines[ lineIndex ];
                var lineNumber = lineIndex + 1;
                var character  = diagnostic.Span.Start - line.Start + 1;
                var prefixSpan = TextSpan.FromBounds( line.Start , diagnostic.Span.Start );
                var suffixSpan = TextSpan.FromBounds( diagnostic.Span.End , line.End );
                var prefix     = syntaxTree.Text.ToString( prefixSpan );
                var error      = syntaxTree.Text.ToString( diagnostic.Span );
                var suffix     = syntaxTree.Text.ToString( suffixSpan );

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write( $"({lineNumber}, {character}): " );
                Console.WriteLine( diagnostic );
                Console.ResetColor();
                Console.Write( "    " );
                Console.Write( prefix );
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write( error );
                Console.ResetColor();
                Console.Write( suffix );
                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
}
