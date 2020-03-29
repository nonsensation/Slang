using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis.Syntax
{
    public class SyntaxTree
    {
        private delegate void ParseHandler( SyntaxTree syntaxTree ,
                                            out CompilationUnitSyntax compilationUnit ,
                                            out ImmutableArray<Diagnostic> diagnostics );

        private SyntaxTree( SourceText text , ParseHandler handler )
        {
            this.Text = text;

            var parser = new Parser( this );

            handler( this , out var root , out var diagnostics );

            this.Root = root;
            this.Diagnostics = diagnostics;
        }

        public SourceText Text { get; }
        public ImmutableArray<Diagnostic> Diagnostics { get; }
        public CompilationUnitSyntax Root { get; }

        public static SyntaxTree Load( string fileName )
        {
            var text = File.ReadAllText( fileName );
            var sourceText = SourceText.From( text , fileName );

            return Parse( sourceText );
        }

        public static void Parse( SyntaxTree syntaxTree , out CompilationUnitSyntax root , out ImmutableArray<Diagnostic> diagnostics )
        {
            var parser = new Parser( syntaxTree );

            root = parser.ParseCompilationUnit();
            diagnostics = parser.Diagnostics.ToImmutableArray();
        }

        public static SyntaxTree Parse( SourceText text )
            => new SyntaxTree( text , Parse );

        public static SyntaxTree Parse( string text )
            => new SyntaxTree( SourceText.From( text ) , Parse );

        public static ImmutableArray<SyntaxToken> ParseTokens( string text )
        {
            var sourceText = SourceText.From( text );

            return ParseTokens( sourceText );
        }

        public static ImmutableArray<SyntaxToken> ParseTokens( string text , out ImmutableArray<Diagnostic> diagnostics )
        {
            var sourceText = SourceText.From( text );

            return ParseTokens( sourceText , out diagnostics );
        }

        public static ImmutableArray<SyntaxToken> ParseTokens( SourceText text )
        {
            return ParseTokens( text , out _ );
        }

        public static ImmutableArray<SyntaxToken> ParseTokens( SourceText text , out ImmutableArray<Diagnostic> diagnostics )
        {
            var tokens = new List<SyntaxToken>();

            void ParseTokens( SyntaxTree syntaxTree , out CompilationUnitSyntax root , out ImmutableArray<Diagnostic> diagnostics )
            {
                var lexer = new Lexer( syntaxTree );

                while( true )
                {
                    var token = lexer.Lex();

                    if( token.TkType == TokenType.EndOfFile )
                    {
                        root = new CompilationUnitSyntax( syntaxTree , ImmutableArray<MemberSyntax>.Empty , token );

                        break;
                    }

                    tokens.Add( token );
                }

                diagnostics = lexer.Diagnostics.ToImmutableArray();
            }

            var tree = new SyntaxTree( text , ParseTokens );

            diagnostics = tree.Diagnostics.ToImmutableArray();

            return tokens.ToImmutableArray();
        }
    }
}
