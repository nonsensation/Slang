using System;
using System.Collections.Immutable;
using System.Diagnostics;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis
{
    internal sealed class Lexer
    {
        private readonly SyntaxTree SyntaxTree;
        private SourceText Text => this.SyntaxTree.Text;
        private int Position;
        private SyntaxKind Kind;
        public DiagnosticBag Diagnostics = new DiagnosticBag();
        private int StartPos;
        private ImmutableArray<SyntaxTrivia>.Builder TriviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();

        public Lexer( SyntaxTree syntaxTree )
        {
            this.SyntaxTree = syntaxTree;
        }

        private char Current => this.Peek( 0 );

        private void Advance( int count = 1 )
            => this.Position += count;

        private char Peek( int offset )
        {
            var index = this.Position + offset;

            if( index >= this.Text.Length )
            {
                return '\0';
            }

            return this.Text[ index ];
        }

        public SyntaxToken Lex()
        {
            this.ReadTrivia( isLeading: true );

            var leadingTrivia = this.TriviaBuilder.ToImmutable();
            var tokenStartPos = this.Position;

            this.ReadToken();

            var tokenKind = this.Kind;
            var tokenLength = this.Position - this.StartPos;

            this.ReadTrivia( isLeading: false );

            var trailingTrivia = this.TriviaBuilder.ToImmutable();
            var tokenText = this.Kind.GetText();

            if( tokenText == null )
            {
                tokenText = this.Text.ToString( tokenStartPos , tokenLength );
            }

            return new SyntaxToken( this.SyntaxTree , tokenKind , tokenStartPos ,
                                    tokenText , leadingTrivia , trailingTrivia );
        }

        public void ReadToken()
        {
            this.StartPos = this.Position;
            this.Kind     = SyntaxKind.None;

            switch( this.Current )
            {
                case '\n':
                case ' ':
                case '\r':
                case '\t':
                case '#':
                {
                    throw new Exception( "should be handled in ReadTrivia()" );
                }

                case '\0':
                {
                    this.Kind = SyntaxKind.EndOfFile;
                    break;
                }

                #region Single character punctuators
                case '(':
                {
                    this.Kind = SyntaxKind.OpenParenToken;
                    this.Advance();
                    break;
                }
                case ')':
                {
                    this.Kind = SyntaxKind.CloseParenToken;
                    this.Advance();
                    break;
                }
                case '{':
                {
                    this.Kind = SyntaxKind.OpenBraceToken;
                    this.Advance();
                    break;
                }
                case '}':
                {
                    this.Kind = SyntaxKind.CloseBraceToken;
                    this.Advance();
                    break;
                }
                // case '[':
                // {
                //     this.Kind = SyntaxKind.OpenBracketToken;
                //     this.Advance();
                //     break;
                // }
                // case ']':
                // {
                //     this.Kind = SyntaxKind.CloseBracketToken;
                //     this.Advance();
                //     break;
                // }
                case ',':
                {
                    this.Kind = SyntaxKind.CommaToken;
                    this.Advance();
                    break;
                }
                case ':':
                {
                    this.Kind = SyntaxKind.ColonToken;
                    this.Advance();
                    break;
                }
                // case ';':
                // {
                //     this.Kind = SyntaxKind.SemicolonToken;
                //     this.Advance();
                //     break;
                // }
                case '_':
                {
                    this.Kind = SyntaxKind.UnderscoreToken;
                    this.Advance();
                    break;
                }

                #endregion

                #region multi character punctuators
                // case '.':
                // {
                //     this.Advance();

                //     if( this.Current == '.' )
                //     {
                //         this.Advance();

                //         if( this.Current == '.' )
                //         {
                //             this.Kind = SyntaxKind.DotDotDotToken;
                //             this.Advance();
                //         }
                //         else
                //         {
                //             this.Kind = SyntaxKind.DotDotToken;
                //         }
                //     }
                //     else
                //     {
                //         this.Kind = SyntaxKind.DotToken;
                //     }
                //     break;
                // }
                case '+':
                {
                    this.Advance();

                    // if( this.Current == '=' )
                    // {
                    //     // this.Kind = SyntaxKind.PlusEq;
                    //     // this.Advance();
                    // }
                    // else
                    {
                        this.Kind = SyntaxKind.PlusToken;
                    }
                    break;
                }
                case '-':
                {
                    this.Advance();

                    // if( this.Current == '=' )
                    // {
                    //     // this.Kind = SyntaxKind.MinusEqToken;
                    //     // this.Advance();
                    // }
                    // else
                    {
                        this.Kind = SyntaxKind.MinusToken;
                    }
                    break;
                }
                case '*':
                {
                    this.Advance();

                    // if( this.Current == '=' )
                    // {
                    //     // this.Kind = SyntaxKind.StarEqToken;
                    //     // this.Advance();
                    // }
                    // else
                    {
                        this.Kind = SyntaxKind.StarToken;
                    }
                    break;
                }
                case '/':
                {
                    this.Advance();

                    // if( this.Current == '=' )
                    // {
                    //     // this.Kind = SyntaxKind.SlashEqToken;
                    //     // this.Advance();
                    // }
                    // else
                    {
                        this.Kind = SyntaxKind.SlashToken;
                    }
                    break;
                }
                case '&':
                {
                    this.Advance();

                    if( this.Current == '&' )
                    {
                        this.Kind = SyntaxKind.AndAndToken;
                        this.Advance();
                    }
                    else
                    {
                        goto default;
                        // this.Kind = SyntaxKind.AndToken;
                    }
                    break;
                }
                case '|':
                {
                    this.Advance();

                    if( this.Current == '|' )
                    {
                        this.Kind = SyntaxKind.PipePipeToken;
                        this.Advance();
                    }
                    else
                    {
                        goto default;
                        // this.Kind = SyntaxKind.PipeToken;
                    }
                    break;
                }
                case '=':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.Kind = SyntaxKind.EqEqToken;
                        this.Advance();
                    }
                    else if( this.Current == '>' )
                    {
                        this.Kind = SyntaxKind.EqGtToken;
                        this.Advance();
                    }
                    else
                    {
                        this.Kind = SyntaxKind.EqToken;
                    }
                    break;
                }
                case '!':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.Kind = SyntaxKind.ExmEqToken;
                        this.Advance();
                    }
                    else
                    {
                        this.Kind = SyntaxKind.ExmToken;
                    }
                    break;
                }
                case '>':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.Kind = SyntaxKind.GtEqToken;
                        this.Advance();
                    }
                    else
                    {
                        this.Kind = SyntaxKind.GtToken;
                    }
                    break;
                }
                case '<':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.Kind = SyntaxKind.LtEqToken;
                        this.Advance();
                    }
                    else
                    {
                        this.Kind = SyntaxKind.LtToken;
                    }
                    break;
                }
                #endregion

                #region case '0' .. '9':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                {
                    this.ReadNumber();
                    break;
                }
                #endregion

                #region case 'a'..'z' && 'A'..'Z':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                {
                    this.ReadIdentifierOrKeyword();
                    break;
                }
                #endregion

                case '"':
                {
                    this.ReadString();
                    break;
                }

                default:
                {
                    var span = new TextSpan( this.Position , 1 );
                    var location = new TextLocation( this.Text , span );

                    this.Diagnostics.ReportBadCharacter( location , this.Current );
                    this.Advance();

                    break;
                }
            }
        }

        private void ReadString()
        {
            this.Advance(); // Skip the current quote

            var done = false;

            while( !done )
            {
                switch( this.Current )
                {
                    case '\0':
                    case '\r':
                    case '\n':
                    {
                        var span = new TextSpan( this.StartPos , 1 );
                        var location = new TextLocation( this.Text , span );

                        this.Diagnostics.ReportUnterminatedString( location );

                        done = true;
                    }
                    break;
                    case '"':
                        if( this.Peek( 1 ) == '"' )
                        {
                            this.Advance( 2 );
                        }
                        else
                        {
                            this.Advance( 1 );

                            done = true;
                        }
                        break;
                    default:
                        this.Advance( 1 );
                        break;
                }
            }

            this.Kind = SyntaxKind.StringLiteral;
        }

        private void ReadWhiteSpaceTrivia()
        {
            var done = false;

            while( !done )
            {
                switch( this.Current )
                {
                    case '\0':
                    case '\r':
                    case '\n':
                    {
                        done = true;
                    }
                    break;
                    default:
                    {
                        if( !char.IsWhiteSpace( Current ) )
                        {
                            done = true;
                        }
                        else
                        {
                            this.Advance();
                        }
                    }
                    break;
                }
            }

            this.Kind = SyntaxKind.WhiteSpaceTrivia;
        }

        private void ReadNumber()
        {
            while( char.IsDigit( this.Current ) )
            {
                this.Advance();
            }

            // var length = this.Position - this.StartPos;
            // var text = this.Text.ToString( this.StartPos , length );

            this.Kind = SyntaxKind.NumericLiteral;
        }

        private void ReadIdentifierOrKeyword()
        {
            Debug.Assert( char.IsLetter( this.Current ) ); // disallow identifiers with leading underscore (or numbers)

            while( char.IsLetterOrDigit( this.Current ) || this.Current == '_' )
            {
                this.Advance();
            }

            var length = this.Position - this.StartPos;
            var text   = this.Text.ToString( this.StartPos , length );

            this.Kind = SyntaxInfo.GetSyntaxKind( text ) ?? SyntaxKind.Identifier;
        }

        private void ReadTrivia( bool isLeading )
        {
            this.TriviaBuilder.Clear();

            var done = false;

            while( !done )
            {
                this.Kind = SyntaxKind.None;
                this.StartPos = this.Position;

                switch( this.Current )
                {
                    case '\0':
                    {
                        done = true;
                    }
                    break;
                    case '#':
                    {
                        this.ReadCommentTrivia();
                    }
                    break;
                    case '\n':
                    case '\r':
                    {
                        if( !isLeading )
                        {
                            done = true;
                        }

                        this.ReadNewLineWhiteSpaceTrivia();
                    }
                    break;
                    case ' ':
                    case '\t':
                    {
                        this.ReadWhiteSpaceTrivia();
                    }
                    break;
                    default:
                    {
                        if( char.IsWhiteSpace( this.Current ) )
                        {
                            this.ReadWhiteSpaceTrivia();
                        }
                        else
                        {
                            done = true;
                        }
                    }
                    break;
                }

                var length = this.Position - this.StartPos;

                if( length > 0 )
                {
                    var text = this.Text.ToString( this.StartPos , length );
                    var trivia = new SyntaxTrivia( this.SyntaxTree , this.Kind , this.StartPos , text );

                    this.TriviaBuilder.Add( trivia );
                }
            }
        }

        private void ReadNewLineWhiteSpaceTrivia()
        {
            if( this.Current == '\r' && this.Peek( 1 ) == '\n' )
            {
                this.Advance( 2 );
            }
            else
            {
                this.Advance( 1 );
            }

            this.Kind = SyntaxKind.NewLineWhiteSpaceTrivia;
        }

        private void ReadCommentTrivia()
        {
            Debug.Assert( this.Current == '#' );

            this.Kind = SyntaxKind.None; // BadTrivia?

            var done = false;

            while( !done )
            {
                switch( this.Current )
                {
                    case '\0':
                    case '\r':
                    case '\n':
                    {
                        done = true;
                        break;
                    }
                    default:
                    {
                        this.Advance();
                        break;
                    }
                }
            }

            this.Kind = SyntaxKind.CommentTrivia;
        }
    }
}
