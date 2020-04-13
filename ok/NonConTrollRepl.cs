using System;
using System.Collections.Generic;
using System.Linq;
using NonConTroll.CodeAnalysis;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.IO;
using System.IO;

namespace NonConTroll
{
    internal class NonConTrollRepl : Repl
    {
        private Compilation? Previous;
        private bool ShowTree;
        private bool ShowProgram;
        private bool LoadingSubmissions;
        private readonly Dictionary<VariableSymbol, object> Variables = new Dictionary<VariableSymbol, object>();

        private static readonly Compilation EmptyCompilation = new Compilation();

        public NonConTrollRepl()
        {
            this.LoadSubmissions();
        }

        protected override void RenderLine( string line )
        {
            var tokens = SyntaxTree.ParseTokens( line );

            foreach( var token in tokens )
            {
                if(      token.TkType.IsTokenKind( TokenKind.Keyword ) )     Console.ForegroundColor = ConsoleColor.DarkMagenta;
                else if( token.TkType.IsTokenKind( TokenKind.Punctuation ) ) Console.ForegroundColor = ConsoleColor.DarkGray;
                else if( token.TkType.IsTokenKind( TokenKind.Identifier ) )  Console.ForegroundColor = ConsoleColor.DarkCyan;
                else if( token.TkType == TokenType.NumericLiteral )          Console.ForegroundColor = ConsoleColor.DarkGreen;
                else if( token.TkType == TokenType.StringLiteral )           Console.ForegroundColor = ConsoleColor.DarkYellow;
                else                                                         Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write( token.Text );
                Console.ResetColor();
            }
        }

        [MetaCommand( "tree" , "Shows the parse tree" )]
        protected void Evaluate_Tree()
        {
            this.ShowTree = !this.ShowTree;

            Console.WriteLine( this.ShowTree ? "Showing parse trees." : "Not showing parse trees." );
        }

        [MetaCommand( "program" , "Shows the bound tree" )]
        protected void Evaluate_Program()
        {
            this.ShowProgram = !this.ShowProgram;

            Console.WriteLine( this.ShowProgram ? "Showing bound tree." : "Not showing bound tree." );
        }

        [MetaCommand( "cls" , "Clear screen" )]
        protected void Evaluate_Cls()
        {
            Console.Clear();
        }

        [MetaCommand( "reset" , "Reset the REPL" )]
        protected void Evaluate_Reset()
        {
            this.Previous = null;
            this.Variables.Clear();
            this.ClearSubmissions();
        }

        [MetaCommand( "load" , "Loads a script file" )]
        protected void Evaluate_Load( string path )
        {
            if( !File.Exists( path ) )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine( $"error: file '{path}' does not exist!" );
                Console.ResetColor();

                return;
            }

            var file = File.ReadAllText( path );

            this.EvaluateSubmission( file );
        }

        [MetaCommand( "symbols" , "Lists all symbols" )]
        protected void Evaluate_Symbols()
        {
            var compilation = this.Previous ?? EmptyCompilation;
            var symbols = compilation.GetSymbols().OrderBy( x => x.Kind ).ThenBy( x => x.Name );

            foreach( var symbol in symbols )
            {
                symbol.WriteTo( Console.Out );

                Console.WriteLine();
            }
        }

        protected override bool IsCompleteSubmission( string text )
        {
            if( string.IsNullOrEmpty( text ) )
                return true;

            var lastTwoLinesAreBlank = text
                .Split( Environment.NewLine )
                .Reverse()
                .TakeWhile( s => string.IsNullOrEmpty( s ) )
                .Take( 2 )
                .Count() == 2;

            if( lastTwoLinesAreBlank )
                return true;

            var syntaxTree = SyntaxTree.Parse( text );

            // Use Members because we need to exclude the EndOfFileToken.
            if( !syntaxTree.Root.Members.Any() ||
                syntaxTree.Root.Members.Last().GetLastToken().IsMissing )
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

                this.SaveSubmission( text );
            }
            else
            {
                Console.Error.WriteDiagnostics( result.Diagnostics );
            }
        }

        private string GetSubmissionDirectory()
        {
            var localAppData = Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );

            return Path.Combine( localAppData , "Slang" , "Submissions" );
        }

        private void LoadSubmissions()
        {
            var submissionsDir = this.GetSubmissionDirectory();

            if( !Directory.Exists( submissionsDir ) )
                return;

            var files = Directory.GetFiles( submissionsDir ).OrderBy( x => x ).ToArray();

            if( !files.Any() )
                return;

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine( $"Loaded {files.Length} submissions" );
            Console.ResetColor();

            this.LoadingSubmissions = true;

            foreach( var file in files )
            {
                var text = File.ReadAllText( file );

                this.EvaluateSubmission( text );
            }

            this.LoadingSubmissions = false;
        }


        private void ClearSubmissions()
        {
            Directory.Delete( this.GetSubmissionDirectory() , recursive: true );
        }

        private void SaveSubmission( string text )
        {
            if( this.LoadingSubmissions )
                return;

            var submissionsDir = this.GetSubmissionDirectory();

            Directory.CreateDirectory( submissionsDir );

            var count = Directory.GetFiles( submissionsDir ).Length;
            var name = $"Submission{count:0000}";
            var fileName = Path.Combine( submissionsDir , name );

            File.WriteAllText( fileName , text );
        }
    }
}
