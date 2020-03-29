using System;
using System.Diagnostics;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.Text;

namespace NonConTroll.CodeAnalysis
{
    public class Lexer
    {
        private readonly SyntaxTree SyntaxTree;
        private SourceText Text => this.SyntaxTree.Text;
        private int Position;
        private TokenType TkType;
        public DiagnosticBag Diagnostics = new DiagnosticBag();
        private int StartPos;

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
                return '\0';

            return this.Text[ index ];
        }

        public SyntaxToken Lex()
        {
            this.StartPos = this.Position;
            this.TkType = TokenType.None;

            switch( this.Current )
            {
                case '\n':
                case ' ':
                case '\r':
                case '\t':
                {
                    this.ScanWhiteSpace();
                    break;
                }

                case '\0':
                {
                    this.TkType = TokenType.EndOfFile;
                    break;
                }

                #region Single character punctuators
                case '(':
                {
                    this.TkType = TokenType.OpenParen;
                    this.Advance();
                    break;
                }
                case ')':
                {
                    this.TkType = TokenType.CloseParen;
                    this.Advance();
                    break;
                }
                case '{':
                {
                    this.TkType = TokenType.OpenBrace;
                    this.Advance();
                    break;
                }
                case '}':
                {
                    this.TkType = TokenType.CloseBrace;
                    this.Advance();
                    break;
                }
                case '[':
                {
                    this.TkType = TokenType.OpenBracket;
                    this.Advance();
                    break;
                }
                case ']':
                {
                    this.TkType = TokenType.CloseBracket;
                    this.Advance();
                    break;
                }
                case ',':
                {
                    this.TkType = TokenType.Comma;
                    this.Advance();
                    break;
                }
                case ':':
                {
                    this.TkType = TokenType.Colon;
                    this.Advance();
                    break;
                }
                case ';':
                {
                    this.TkType = TokenType.Semicolon;
                    this.Advance();
                    break;
                }
                #endregion

                #region multi character punctuators
                case '.':
                {
                    this.Advance();

                    if( this.Current == '.' )
                    {
                        this.Advance();

                        if( this.Current == '.' )
                        {
                            this.TkType = TokenType.DotDotDot;
                            this.Advance();
                        }
                        else
                        {
                            this.TkType = TokenType.DotDot;
                        }
                    }
                    else
                    {
                        this.TkType = TokenType.Dot;
                    }
                    break;
                }
                case '+':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.TkType = TokenType.PlusEq;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Plus;
                    }
                    break;
                }
                case '-':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.TkType = TokenType.MinusEq;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Minus;
                    }
                    break;
                }
                case '*':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.TkType = TokenType.StarEq;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Star;
                    }
                    break;
                }
                case '/':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.TkType = TokenType.SlashEq;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Slash;
                    }
                    break;
                }
                case '&':
                {
                    this.Advance();

                    if( this.Current == '&' )
                    {
                        this.TkType = TokenType.AndAnd;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.And;
                    }
                    break;
                }
                case '|':
                {
                    this.Advance();

                    if( this.Current == '|' )
                    {
                        this.TkType = TokenType.PipePipe;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Pipe;
                    }
                    break;
                }
                case '=':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.TkType = TokenType.EqEq;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Eq;
                    }
                    break;
                }
                case '!':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.TkType = TokenType.ExmEq;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Exm;
                    }
                    break;
                }
                case '>':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.TkType = TokenType.GtEq;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Gt;
                    }
                    break;
                }
                case '<':
                {
                    this.Advance();

                    if( this.Current == '=' )
                    {
                        this.TkType = TokenType.LtEq;
                        this.Advance();
                    }
                    else
                    {
                        this.TkType = TokenType.Lt;
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
                    this.ScanNumber();
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
                    this.ScanIdentifierOrKeyword();
                    break;
                }
                #endregion

                case '"':
                {
                    this.ScanString();
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

            var length = this.Position - this.StartPos;
            var text = this.TkType.GetName();

            if( text == null )
                text = this.Text.ToString( this.StartPos , length );

            return new SyntaxToken( this.SyntaxTree , this.TkType , this.StartPos , text );
        }

        private void ScanString()
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

            this.TkType = TokenType.StringLiteral;
        }

        private void ScanWhiteSpace()
        {
            while( char.IsWhiteSpace( this.Current ) )
                this.Advance( 1 );


            this.TkType = TokenType.WhiteSpace;

        }

        private void ScanNumber()
        {
            while( char.IsDigit( this.Current ) )
                this.Advance();

            // var length = this.Position - this.StartPos;
            // var text = this.Text.ToString( this.StartPos , length );

            this.TkType = TokenType.NumericLiteral;
        }

        private void ScanIdentifierOrKeyword()
        {
            while( char.IsLetter( this.Current ) )
                this.Advance();

            var length = this.Position - this.StartPos;
            var text = this.Text.ToString( this.StartPos , length );

            this.TkType = SyntaxInfo.GetKeywordTokenType( text );
        }
    }
}
