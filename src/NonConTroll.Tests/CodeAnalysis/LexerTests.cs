using System;
using System.Collections.Generic;
using System.Linq;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;
using Xunit;

namespace NonConTroll.Tests.CodeAnalysis.Parser
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_Lexes_UnterminatedString()
        {
            var text = "\"text";
            var tokens = SyntaxTree.ParseTokens( text , out var diagnostics );

            var token = Assert.Single( tokens );

            Assert.Equal( SyntaxKind.StringLiteral , token.Kind );
            Assert.Equal( text , token.Text );

            var diagnostic = Assert.Single( diagnostics );

            Assert.Equal( new TextSpan( 0 , 1 ) , diagnostic.Location.Span );
            Assert.Equal( "Unterminated string literal." , diagnostic.Msg );
        }

        [Fact]
        public void Lexer_Covers_AllTokens()
        {
            var tokens = GetTokens();
            var separators = GetSeparators();
            var testedTokenKinds = tokens.Concat( separators ).Select( t => t.kind );
            var punctuators = Enum.GetValues( typeof( SyntaxKind ) ).Cast<SyntaxKind>().Where( k => k.IsPunctuation() );
            var untestedTokenKinds = new SortedSet<SyntaxKind>( punctuators );

            untestedTokenKinds.Remove( SyntaxKind.None );
            untestedTokenKinds.Remove( SyntaxKind.EndOfFile );
            untestedTokenKinds.ExceptWith( testedTokenKinds );

            Assert.Empty( untestedTokenKinds );
        }

        [Theory]
        [MemberData( nameof( GetTokensData ) )]
        public void Lexer_Lexes_Token( SyntaxKind kind , string text )
        {
            var tokens = SyntaxTree.ParseTokens( text );

            var token = Assert.Single( tokens );

            Assert.Equal( kind , token.Kind );
            Assert.Equal( text , token.Text );
        }

        [Theory]
        [MemberData( nameof( GetSeparatorsData ) )]
        public void Lexer_Lexes_Separator( SyntaxKind kind , string text )
        {
            var tokens = SyntaxTree.ParseTokens( text , includeEndOfFile: true );

            var token = Assert.Single( tokens );
            var trivia = Assert.Single( token.LeadingTrivia );

            Assert.Equal( kind , trivia.Kind );
            Assert.Equal( text , trivia.Text );
        }

        [Theory]
        [MemberData( nameof( GetTokenPairsData ) )]
        public void Lexer_Lexes_TokenPairs( SyntaxKind t1Kind , string t1Text ,
                                            SyntaxKind t2Kind , string t2Text )
        {
            var text = t1Text + t2Text;
            var tokens = SyntaxTree.ParseTokens( text ).ToArray();

            Assert.Equal( 2 , tokens.Length );

            var t1 = tokens[ 0 ];
            var t2 = tokens[ 1 ];

            Assert.Equal( t1Kind , t1.Kind );
            Assert.Equal( t1Text , t1.Text );
            Assert.Equal( t2Kind , t2.Kind );
            Assert.Equal( t2Text , t2.Text );
        }

        [Theory]
        [MemberData( nameof( GetTokenPairsWithSeparatorData ) )]
        public void Lexer_Lexes_TokenPairs_WithSeparators( SyntaxKind t1Kind , string t1Text ,
                                                           SyntaxKind separatorKind , string separatorText ,
                                                           SyntaxKind t2Kind , string t2Text )
        {
            var text = t1Text + separatorText + t2Text;
            var tokens = SyntaxTree.ParseTokens( text ).ToArray();

            Assert.Equal( 2 , tokens.Length );

            var t1 = tokens[ 0 ];
            var t2 = tokens[ 1 ];

            Assert.Equal( t1Kind , t1.Kind );
            Assert.Equal( t1Text , t1.Text );
            Assert.Equal( t2Kind , t2.Kind );
            Assert.Equal( t2Text , t2.Text );

            var separator = Assert.Single( t1.TrailingTrivia );

            Assert.Equal( separatorKind , separator.Kind );
            Assert.Equal( separatorText , separator.Text );
        }

        [Theory]
        [InlineData( "foo" )]
        [InlineData( "foo42" )]
        [InlineData( "foo_42" )]
        public void Lexer_Lexes_Identifiers( string name )
        {
            var tokens = SyntaxTree.ParseTokens( name ).ToArray();

            Assert.Single( tokens );

            var token = tokens[ 0 ];

            Assert.Equal( SyntaxKind.Identifier , token.Kind );
            Assert.Equal( name , token.Text );
        }

        public static IEnumerable<object[]> GetTokensData()
        {
            var tokens = GetTokens();

            foreach( var (kind, text) in tokens )
            {
                yield return new object[] { kind , text };
            }
        }

        public static IEnumerable<object[]> GetSeparatorsData()
        {
            var separators = GetSeparators();

            foreach( var (kind, text) in separators )
            {
                yield return new object[] { kind , text };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsData()
        {
            var pairs = GetTokenPairs();

            foreach( var (t1Kind, t1Text, t2Kind, t2Text) in pairs )
            {
                yield return new object[] { t1Kind , t1Text , t2Kind , t2Text };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
        {
            var pairs = GetTokenPairsWithSeparator();

            foreach( var (t1Kind, t1Text, separatorKind, separatorText, t2Kind, t2Text) in pairs )
            {
                yield return new object[] { t1Kind , t1Text , separatorKind , separatorText , t2Kind , t2Text };
            }
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetTokens()
        {
            var fixedTokens = Enum.GetValues( typeof( SyntaxKind ) ).Cast<SyntaxKind>()
                .Select( k => (kind: k, text: SyntaxInfo.GetText( k )) )
                .Where( t => t.text != null );


            var dynamicTokens = new[]
            {
                (SyntaxKind.NumericLiteral, "1"),
                (SyntaxKind.NumericLiteral, "123"),
                (SyntaxKind.Identifier, "a"),
                (SyntaxKind.Identifier, "abc"),
                (SyntaxKind.StringLiteral, "\"Test\""),
                (SyntaxKind.StringLiteral, "\"Te\"\"st\""),
            };

            return fixedTokens.Concat( dynamicTokens );
        }

        private static IEnumerable<(SyntaxKind kind, string text)> GetSeparators()
        {
            return new []
            {
                (SyntaxKind.WhiteSpaceTrivia, " "),
                (SyntaxKind.WhiteSpaceTrivia, "  "),
                (SyntaxKind.NewLineWhiteSpaceTrivia, "\r"),
                (SyntaxKind.NewLineWhiteSpaceTrivia, "\n"),
                (SyntaxKind.NewLineWhiteSpaceTrivia, "\r\n"),
                // (SyntaxKind.MultiLineCommentTrivia, "/**/"),
            };
        }

        private static bool RequiresSeparator( SyntaxKind t1Kind , SyntaxKind t2Kind )
        {
            // TODO: what is double-underscore supposed to mean/what error?
            // right now its literally 2 underscore tokens, maybe have something better

            var pairs = new List<(SyntaxKind, SyntaxKind)> {
                (SyntaxKind.Identifier, SyntaxKind.Identifier),
                (SyntaxKind.Identifier, SyntaxKind.UnderscoreToken),
                (SyntaxKind.Identifier, SyntaxKind.NumericLiteral),

                (SyntaxKind.NumericLiteral, SyntaxKind.NumericLiteral),

                (SyntaxKind.StringLiteral, SyntaxKind.StringLiteral),

                (SyntaxKind.GtToken, SyntaxKind.EqToken),
                (SyntaxKind.GtToken, SyntaxKind.EqEqToken),
                (SyntaxKind.GtToken, SyntaxKind.EqGtToken),

                (SyntaxKind.EqToken, SyntaxKind.EqToken),
                (SyntaxKind.EqToken, SyntaxKind.EqEqToken),
                (SyntaxKind.EqToken, SyntaxKind.EqGtToken),
                (SyntaxKind.EqToken, SyntaxKind.GtToken),
                (SyntaxKind.EqToken, SyntaxKind.GtEqToken),

                (SyntaxKind.ExmToken, SyntaxKind.EqToken),
                (SyntaxKind.ExmToken, SyntaxKind.EqEqToken),
                (SyntaxKind.ExmToken, SyntaxKind.EqGtToken),

                (SyntaxKind.LtToken, SyntaxKind.EqToken),
                (SyntaxKind.LtToken, SyntaxKind.EqEqToken),
                (SyntaxKind.LtToken, SyntaxKind.EqGtToken),
            };

            foreach( var (lhsKind, rhsKind) in pairs )
            {
                if( lhsKind == t1Kind && rhsKind == t2Kind )
                {
                    return true;
                }
            }

            var t1IsKeyword = t1Kind.IsKeyword();
            var t2IsKeyword = t2Kind.IsKeyword();

            if( t1IsKeyword )
            {
                if( t2IsKeyword ||
                    t2Kind == SyntaxKind.Identifier ||
                    t2Kind == SyntaxKind.UnderscoreToken ||
                    t2Kind == SyntaxKind.NumericLiteral )
                {
                    return true;
                }
            }
            else if( t2IsKeyword )
            {
                if( t1Kind == SyntaxKind.Identifier )
                {
                    return true;
                }
            }

            // else if( t1Kind == SyntaxKind.AndToken && t2Kind == SyntaxKind.AndToken )             { return true; }
            // else if( t1Kind == SyntaxKind.AndToken && t2Kind == SyntaxKind.AndAndToken )          { return true; }
            // else if( t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipeToken )           { return true; }
            // else if( t1Kind == SyntaxKind.PipeToken && t2Kind == SyntaxKind.PipePipeToken )       { return true; }
            // else if( t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.SlashToken )         { return true; }
            // else if( t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.StarToken )          { return true; }
            // else if( t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.CommentTrivia )      { return true; }


            // if( t1Kind == SyntaxKind.SlashToken && t2Kind == SyntaxKind.MultiLineCommentTrivia )
            // {
            //     return true;
            // }

            return false;
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
        {
            var tokens = GetTokens();

            foreach( var t1 in tokens )
            {
                foreach( var t2 in tokens )
                {
                    if( !RequiresSeparator( t1.kind , t2.kind ) )
                    {
                        yield return (t1.kind, t1.text, t2.kind, t2.text);
                    }
                }
            }
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind separatorKind, string separatorText, SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
        {
            var tokens = GetTokens();
            var seperators = GetSeparators();

            foreach( var t1 in tokens )
            {
                foreach( var t2 in tokens )
                {
                    if( RequiresSeparator( t1.kind , t2.kind ) )
                    {
                        foreach( var s in seperators )
                        {
                            if( !RequiresSeparator( t1.kind , s.kind ) &&
                                !RequiresSeparator( s.kind , t2.kind ) )
                            {
                                yield return (t1.kind, t1.text, s.kind, s.text, t2.kind, t2.text);
                            }
                        }
                    }
                }
            }
        }
    }
}
