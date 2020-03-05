using System;
using System.Collections.Generic;
using System.Linq;
using NonConTroll.CodeAnalysis;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll
{
    internal sealed class NonConTrollRepl : Repl
    {
        private Compilation? Previous;
        private bool ShowTree;
        private bool ShowProgram;
        private readonly Dictionary<VariableSymbol, object> Variables = new Dictionary<VariableSymbol, object>();

        protected override void RenderLine( string line )
        {
            var tokens = SyntaxTree.ParseTokens( line );

            foreach( var token in tokens )
            {
                var isKeyword = token.TkType.IsTokenKind( TokenKind.Keyword );
                var isIdentifier = token.TkType == TokenType.Identifier;
                var isNumber = token.TkType == TokenType.NumericLiteral;
                var isString = token.TkType == TokenType.StringLiteral;

                if( isKeyword )
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if( isIdentifier )
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if( isNumber )
                    Console.ForegroundColor = ConsoleColor.Cyan;
                else if( isString )
                    Console.ForegroundColor = ConsoleColor.Magenta;
                else
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write( token.Text );
                Console.ResetColor();
            }
        }

        protected override void EvaluateMetaCommand( string input )
        {
            switch( input )
            {
                case "#showTree":
                    this.ShowTree = !this.ShowTree;
                    Console.WriteLine( this.ShowTree ? "Showing parse trees." : "Not showing parse trees." );
                    break;
                case "#showProgram":
                    this.ShowProgram = !this.ShowProgram;
                    Console.WriteLine( this.ShowProgram ? "Showing bound tree." : "Not showing bound tree." );
                    break;
                case "#cls":
                    Console.Clear();
                    break;
                case "#reset":
                    this.Previous = null;
                    this.Variables.Clear();
                    break;
                default:
                    base.EvaluateMetaCommand( input );
                    break;
            }
        }

        protected override bool IsCompleteSubmission( string text )
        {
            if( string.IsNullOrEmpty( text ) )
                return true;

            var lastTwoLinesAreBlank = text
                .Split(Environment.NewLine).Reverse()
                .TakeWhile( s => string.IsNullOrEmpty( s ) )
                .Take( 2 ).Count() == 2;

            if( lastTwoLinesAreBlank )
                return true;

            var syntaxTree = SyntaxTree.Parse(text);

            // Use Members because we need to exclude the EndOfFileToken.
            if( syntaxTree.Root.Members.Last().GetLastToken().IsMissing )
                return false;

            return true;
        }

        protected override void EvaluateSubmission( string text )
        {
            var syntaxTree = SyntaxTree.Parse( text );
            var compilation = this.Previous == null
                ? new Compilation( syntaxTree )
                : this.Previous.ContinueWith( syntaxTree );

            if( this.ShowTree )
                syntaxTree.Root.WriteTo( Console.Out );

            if( this.ShowProgram )
                compilation.EmitTree( Console.Out );

            var result = compilation.Evaluate( this.Variables );

            if( !result.Diagnostics.Any() )
            {
                if( result.Value != null )
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine( result.Value );
                    Console.ResetColor();
                }

                this.Previous = compilation;
            }
            else
            {
                foreach( var diagnostic in result.Diagnostics.OrderBy( diag => diag.Span , new TextSpanComparer() ) )
                {
                    var lineIndex = syntaxTree.Text.GetLineIndex( diagnostic.Span.Start );
                    var line = syntaxTree.Text.Lines[lineIndex];
                    var lineNumber = lineIndex + 1;
                    var character = diagnostic.Span.Start - line.Start + 1;

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write( $"({lineNumber}, {character}): " );
                    Console.WriteLine( diagnostic );
                    Console.ResetColor();

                    var prefixSpan = TextSpan.FromBounds( line.Start , diagnostic.Span.Start );
                    var suffixSpan = TextSpan.FromBounds( diagnostic.Span.End , line.End );
                    var prefix = syntaxTree.Text.ToString( prefixSpan );
                    var error = syntaxTree.Text.ToString( diagnostic.Span );
                    var suffix = syntaxTree.Text.ToString( suffixSpan );

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
}
