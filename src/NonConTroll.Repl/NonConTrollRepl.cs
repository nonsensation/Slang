using System;
using System.Collections.Generic;
using System.Linq;
using NonConTroll.CodeAnalysis;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.IO;
using System.IO;
using System.Collections.Immutable;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll
{
    internal partial class NonConTrollRepl : Repl
    {
        private Compilation? Previous;
        private bool ShowTree;
        private bool ShowProgram;
        private bool LoadingSubmissions;
        private readonly Dictionary<VariableSymbol, object> Variables = new Dictionary<VariableSymbol, object>();

        private static readonly Compilation EmptyCompilation = Compilation.Create( null );

        public NonConTrollRepl()
        {
            this.LoadSubmissions();
        }

        protected override object? RenderLine( IReadOnlyList<string> lines , int lineIndex , object? state  )
        {
            var syntaxTree = default( SyntaxTree );

            if( state == null )
            {
                var text = string.Join( Environment.NewLine , lines );
                var sourceText = SourceText.From( text );

                syntaxTree = SyntaxTree.Parse( sourceText );
            }
            else
            {
                syntaxTree = (SyntaxTree)state;
            }

            var lineSpan = syntaxTree.Text.Lines[ lineIndex ].Span;
            var classifiedSpans = Classifier.Classify( syntaxTree , lineSpan );

            foreach( var classifiedSpan in classifiedSpans )
            {
                var tokenText = syntaxTree.Text.ToString( classifiedSpan.Span );

                switch( classifiedSpan.Classification )
                {
                    case Classification.Text:
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        break;
                    case Classification.Keyword:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case Classification.Identifier:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;
                    case Classification.Number:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case Classification.String:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case Classification.Punctuation:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case Classification.Comment:
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        break;
                    default:
                        throw new Exception();
                }

                Console.Write( tokenText );
                Console.ResetColor();
            }

            return syntaxTree;
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
            {
                return true;
            }

            var lastTwoLinesAreBlank = text
                .Split( Environment.NewLine )
                .Reverse()
                .TakeWhile( s => string.IsNullOrEmpty( s ) )
                .Take( 2 )
                .Count() == 2;

            if( lastTwoLinesAreBlank )
            {
                return true;
            }

            var syntaxTree = SyntaxTree.Parse( text );

            // Use Members because we need to exclude the EndOfFileToken.
            if( !syntaxTree.Root.Members.Any() )
            {
                return false;
            }
            else
            {
                var lastMember = syntaxTree.Root.Members.Last();
                var lastToken = lastMember.GetLastToken();

                if( lastToken.IsMissing )
                {
                    return false;
                }
            }

            return true;
        }

        protected override void EvaluateSubmission( string text )
        {
            var syntaxTree  = SyntaxTree.Parse( text );
            var compilation = Compilation.CreateScript( this.Previous , syntaxTree );

            if( this.ShowTree )
            {
                syntaxTree.Root.WriteTo( Console.Out );
            }

            if( this.ShowProgram )
            {
                compilation.EmitTree( Console.Out );
            }

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
            {
                return;
            }

            var files = Directory.GetFiles( submissionsDir ).OrderBy( x => x ).ToArray();

            if( !files.Any() )
            {
                return;
            }

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
            var dir = this.GetSubmissionDirectory();

            if( Directory.Exists( dir ) )
            {
                Directory.Delete( dir , recursive: true );
            }
        }

        private void SaveSubmission( string text )
        {
            if( this.LoadingSubmissions )
            {
                return;
            }

            var submissionsDir = this.GetSubmissionDirectory();

            Directory.CreateDirectory( submissionsDir );

            var count = Directory.GetFiles( submissionsDir ).Length;
            var name = $"Submission{count:0000}.txt";
            var fileName = Path.Combine( submissionsDir , name );

            File.WriteAllText( fileName , text );
        }

    }
}
