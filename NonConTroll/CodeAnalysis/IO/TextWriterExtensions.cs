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
        private static bool IsConsole( this TextWriter writer )
        {
            if( writer == Console.Out )
            {
                return !Console.IsOutputRedirected;
            }

            if( writer is IndentedTextWriter iw && iw.InnerWriter.IsConsole() )
            {
                return true;
            }

            return false;
        }

        private static void SetForeground( this TextWriter writer , ConsoleColor color )
        {
            if( writer.IsConsole() )
            {
                Console.ForegroundColor = color;
            }
        }

        private static void ResetColor( this TextWriter writer )
        {
            if( writer.IsConsole() )
            {
                Console.ResetColor();
            }
        }

        public static void WriteKeyword( this TextWriter writer , SyntaxKind tokenType )
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

        public static void WritePunctuation( this TextWriter writer , SyntaxKind tokenType )
        {
            writer.WritePunctuation( tokenType.GetName() );
        }

        public static void WritePunctuation( this TextWriter writer , string? text )
        {
            writer.SetForeground( ConsoleColor.DarkGray );
            writer.Write( text );
            writer.ResetColor();
        }

        public static void WriteDiagnostics( this TextWriter writer , IEnumerable<Diagnostic> diagnostics )
        {
            var diags = diagnostics
                .OrderBy( diag => diag.Location.Text.FileName )
                .ThenBy( diag => diag.Location.Span.Start )
                .ThenBy( diag => diag.Location.Span.Length );

            foreach( var diagnostic in diags )
            {
                var span           = diagnostic.Location.Span;
                var text           = diagnostic.Location.Text;
                var lineIndex      = text.GetLineIndex( span.Start );
                var line           = text.Lines[ lineIndex ];
                var prefixSpan     = TextSpan.FromBounds( line.Start , span.Start );
                var suffixSpan     = TextSpan.FromBounds( span.End , line.End );
                var prefix         = text.ToString( prefixSpan );
                var suffix         = text.ToString( suffixSpan );
                var error          = text.ToString( span );
                var fileName       = diagnostic.Location.FileName;
                var startLine      = diagnostic.Location.StartLine + 1;
                var endLine        = diagnostic.Location.EndLine + 1;
                var startCharacter = diagnostic.Location.StartCharacter + 1;
                var endCharacter   = diagnostic.Location.EndCharacter + 1;

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write( $"{fileName}({startLine},{startCharacter},{endLine},{endCharacter}): error: " );
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
