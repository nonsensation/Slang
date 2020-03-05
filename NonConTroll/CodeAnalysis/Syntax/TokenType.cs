using System;
using System.Collections.Generic;
using System.Linq;

namespace NonConTroll.CodeAnalysis.Syntax
{
    [Flags]
    public enum TokenKind
    {
        None          = 0,
        Identifier    = 1 << 0,
        Keyword       = 1 << 1,
        Punctuation   = 1 << 2,
        Literal       = 1 << 3,
        Trivia        = 1 << 4,
        WhiteSpace    = 1 << 5,
        Documentation = 1 << 6,
        Comment       = 1 << 7,
    }

    public enum TokenType
    {
        [TokenInfo( TokenKind.None )] None = 0,
        [TokenInfo( TokenKind.Identifier )] Identifier,

        #region Keywords

        [TokenInfo( TokenKind.Keyword )] Func,
        [TokenInfo( TokenKind.Keyword )] Fn,
        [TokenInfo( TokenKind.Keyword )] Return,
        [TokenInfo( TokenKind.Keyword )] Ret,
        [TokenInfo( TokenKind.Keyword )] Class,
        [TokenInfo( TokenKind.Keyword )] Struct,
        [TokenInfo( TokenKind.Keyword )] Mixin,
        [TokenInfo( TokenKind.Keyword )] Abstract,
        [TokenInfo( TokenKind.Keyword )] Meta,
        [TokenInfo( TokenKind.Keyword )] Template,
        [TokenInfo( TokenKind.Keyword )] Import,
        [TokenInfo( TokenKind.Keyword )] Export,
        [TokenInfo( TokenKind.Keyword )] Implicit,
        [TokenInfo( TokenKind.Keyword )] Explicit,
        [TokenInfo( TokenKind.Keyword )] Internal,
        [TokenInfo( TokenKind.Keyword )] External,
        [TokenInfo( TokenKind.Keyword )] Public,
        [TokenInfo( TokenKind.Keyword )] Pub,
        [TokenInfo( TokenKind.Keyword )] Protected,
        [TokenInfo( TokenKind.Keyword )] Private,
        [TokenInfo( TokenKind.Keyword )] If,
        [TokenInfo( TokenKind.Keyword )] Then,
        [TokenInfo( TokenKind.Keyword )] When,
        [TokenInfo( TokenKind.Keyword )] Else,
        [TokenInfo( TokenKind.Keyword )] Elif,
        [TokenInfo( TokenKind.Keyword )] While,
        [TokenInfo( TokenKind.Keyword )] Loop,
        [TokenInfo( TokenKind.Keyword )] For,
        [TokenInfo( TokenKind.Keyword )] Foreach,
        [TokenInfo( TokenKind.Keyword )] Match,
        [TokenInfo( TokenKind.Keyword )] Switch,
        [TokenInfo( TokenKind.Keyword )] Case,
        [TokenInfo( TokenKind.Keyword )] Default,
        [TokenInfo( TokenKind.Keyword )] Delete,
        [TokenInfo( TokenKind.Keyword )] Except,
        [TokenInfo( TokenKind.Keyword )] Try,
        [TokenInfo( TokenKind.Keyword )] Catch,
        [TokenInfo( TokenKind.Keyword )] Expect,
        [TokenInfo( TokenKind.Keyword )] Ensure,
        [TokenInfo( TokenKind.Keyword )] In,
        [TokenInfo( TokenKind.Keyword )] Out,
        [TokenInfo( TokenKind.Keyword )] Ref,
        [TokenInfo( TokenKind.Keyword )] Val,
        [TokenInfo( TokenKind.Keyword )] Ptr,
        [TokenInfo( TokenKind.Keyword )] Var,
        [TokenInfo( TokenKind.Keyword )] Let,
        [TokenInfo( TokenKind.Keyword )] Const,
        [TokenInfo( TokenKind.Keyword )] Readonly,
        [TokenInfo( TokenKind.Keyword )] Volatile,
        [TokenInfo( TokenKind.Keyword )] Enum,
        [TokenInfo( TokenKind.Keyword )] Union,
        [TokenInfo( TokenKind.Keyword )] Is,
        [TokenInfo( TokenKind.Keyword )] As,
        [TokenInfo( TokenKind.Keyword )] Cast,
        [TokenInfo( TokenKind.Keyword )] Operator,
        [TokenInfo( TokenKind.Keyword )] Namespace,
        [TokenInfo( TokenKind.Keyword )] Package,
        [TokenInfo( TokenKind.Keyword )] Module,
        [TokenInfo( TokenKind.Keyword )] Break,
        [TokenInfo( TokenKind.Keyword )] Continue,
        [TokenInfo( TokenKind.Keyword )] Do,
        [TokenInfo( TokenKind.Keyword )] To,

        #endregion

        #region Keyword Literals

        [TokenInfo( TokenKind.Keyword | TokenKind.Literal )] True,
        [TokenInfo( TokenKind.Keyword | TokenKind.Literal )] False,
        [TokenInfo( TokenKind.Keyword | TokenKind.Literal )] Null,
        [TokenInfo( TokenKind.Keyword | TokenKind.Literal )] Undefined,

        #endregion

        #region Punctuation

        [TokenInfo( "." )] Dot,
        [TokenInfo( ".." )] DotDot,
        [TokenInfo( "..." )] DotDotDot,
        [TokenInfo( ":" )] Colon,
        [TokenInfo( "," )] Comma,
        [TokenInfo( ";" )] Semicolon,
        [TokenInfo( "(" )] OpenParen,
        [TokenInfo( ")" )] CloseParen,
        [TokenInfo( "{" )] OpenBrace,
        [TokenInfo( "}" )] CloseBrace,
        [TokenInfo( "[" )] OpenBracket,
        [TokenInfo( "]" )] CloseBracket,
        [TokenInfo( "-" )] Minus,
        [TokenInfo( "+" )] Plus,
        [TokenInfo( "*" )] Star,
        [TokenInfo( "/" )] Slash,
        [TokenInfo( "=" )] Eq,
        [TokenInfo( "==" )] EqEq,
        [TokenInfo( "!" )] Exm,
        [TokenInfo( "?" )] Qm,
        [TokenInfo( "!=" )] ExmEq,
        [TokenInfo( "|" )] Pipe,
        [TokenInfo( "||" )] PipePipe,
        [TokenInfo( "&" )] And,
        [TokenInfo( "&&" )] AndAnd,
        [TokenInfo( "<" )] Lt,
        [TokenInfo( "<=" )] LtEq,
        [TokenInfo( ">" )] Gt,
        [TokenInfo( ">=" )] GtEq,
        [TokenInfo( "->" )] MinusGt,
        [TokenInfo( "<-" )] LtMinus,
        [TokenInfo( "=>" )] EqGt,
        [TokenInfo( "+=" )] PlusEq,
        [TokenInfo( "-=" )] MinusEq,
        [TokenInfo( "*=" )] StarEq,
        [TokenInfo( "/=" )] SlashEq,
        [TokenInfo( "'" )] SingleQuote,
        [TokenInfo( "\"" )] DoubleQuote,
        [TokenInfo( "%" )] Percent,
        [TokenInfo( "$" )] Dollar,
        [TokenInfo( "^" )] Caret,
        [TokenInfo( "~" )] Tilde,
        [TokenInfo( "#" )] Hashtag,
        [TokenInfo( "_" )] Underscore,
        [TokenInfo( "\\" )] BackSlash,
        [TokenInfo( "`" )] BackTick,
        [TokenInfo( "´" )] FrontTick,
        [TokenInfo( "@" )] At,

        #endregion

        #region Trivia

        [TokenInfo( TokenKind.Trivia | TokenKind.WhiteSpace )] WhiteSpace,
        [TokenInfo( TokenKind.Trivia | TokenKind.WhiteSpace )] EndOfFile,
        [TokenInfo( TokenKind.Trivia | TokenKind.WhiteSpace )] NewLine,
        [TokenInfo( TokenKind.Trivia | TokenKind.WhiteSpace )] IndentationWhiteSpace,
        [TokenInfo( TokenKind.Trivia | TokenKind.WhiteSpace )] AlignmentWhiteSpace,

        [TokenInfo( TokenKind.Trivia | TokenKind.Comment )] Comment,
        [TokenInfo( TokenKind.Trivia | TokenKind.Comment | TokenKind.Documentation )] DocComment,


        #endregion

        #region Literals

        [TokenInfo( TokenKind.Literal )] StringLiteral,
        [TokenInfo( TokenKind.Literal )] BooleanLiteral,
        [TokenInfo( TokenKind.Literal )] NumericLiteral,
        [TokenInfo( TokenKind.Literal )] DecimalLiteral,
        [TokenInfo( TokenKind.Literal )] IntegerLiteral,
        [TokenInfo( TokenKind.Literal )] CharacterLiteral,

        #endregion
    }

    public class TokenInfoAttribute : Attribute
    {

        public TokenKind Kind = TokenKind.None;
        public string? Name;

        public TokenInfoAttribute( TokenKind kind )
        {
            this.Kind = kind;
        }

        public TokenInfoAttribute( string displayName )
        {
            this.Name = displayName;
            this.Kind = TokenKind.Punctuation;
        }
    }

    public static class TokenExtensions
    {
        private static IReadOnlyDictionary<TokenType , string> TokenTypeNameCache
            => Enum.GetValues( typeof( TokenType ) ).Cast<TokenType>()
                .ToDictionary( tt => tt , tt => tt.GetTokenInfoAttribute()?.Name ?? FixKeywordNames( tt ) );

        private static IReadOnlyDictionary<TokenType , TokenKind> TokenTypeKindCache
            => Enum.GetValues( typeof( TokenType ) ).Cast<TokenType>()
                .ToDictionary( tt => tt , tt => tt.GetTokenInfoAttribute()?.Kind ?? TokenKind.None );

        private static string FixKeywordNames( TokenType tt )
            => tt.ToString().Replace( "Kw" , "" ).ToLower();

        public static TokenKind GetTokenKind( this TokenType tt )
            => TokenTypeKindCache[ tt ];

        public static bool IsTokenKind( this TokenType tt , TokenKind tk )
            => tt.GetTokenKind().HasFlag( tk );

        public static string GetName( this TokenType tt )
            => TokenTypeNameCache[ tt ];

        public static TokenInfoAttribute GetTokenInfoAttribute( this TokenType tt )
            => typeof( TokenType ).GetMember( tt.ToString() ).First()
                .GetCustomAttributes( typeof( TokenInfoAttribute ) , inherit: false )
                .Cast<TokenInfoAttribute>().FirstOrDefault();
    }

    public static class SyntaxInfo
    {
        private static IReadOnlyDictionary<string , TokenType> TokenTypeKeywordCache
            => Enum.GetValues( typeof( TokenType ) ).Cast<TokenType>()
                .Where( tt => tt.GetTokenInfoAttribute()?.Kind == TokenKind.Keyword )
                .ToDictionary( tt => tt.ToString().ToLower() , tt => tt );

        public static TokenType GetKeywordTokenType( string name )
            => TokenTypeKeywordCache.TryGetValue( name , out var tokenType )
                ? tokenType : TokenType.Identifier;

        public static int GetUnaryOperatorPrecedence( this TokenType tokenType )
        {
            switch( tokenType )
            {
                case TokenType.Plus:
                case TokenType.Minus:
                case TokenType.Exm:
                case TokenType.Tilde:
                    return 6;
                default:
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence( this TokenType tokenType )
        {
            switch( tokenType )
            {
                case TokenType.Star:
                case TokenType.Slash:
                    return 5;
                case TokenType.Plus:
                case TokenType.Minus:
                    return 4;
                case TokenType.EqEq:
                case TokenType.ExmEq:
                case TokenType.Lt:
                case TokenType.LtEq:
                case TokenType.Gt:
                case TokenType.GtEq:
                    return 3;
                case TokenType.And:
                case TokenType.AndAnd:
                    return 2;
                case TokenType.Pipe:
                case TokenType.PipePipe:
                case TokenType.Caret:
                    return 1;
                default:
                    return 0;
            }
        }

        public static IEnumerable<TokenType> GetUnaryOperatorKinds()
        {
            var kinds = (TokenType[]) Enum.GetValues(typeof(TokenType));
            foreach( var kind in kinds )
            {
                if( GetUnaryOperatorPrecedence( kind ) > 0 )
                    yield return kind;
            }
        }

        public static IEnumerable<TokenType> GetBinaryOperatorKinds()
        {
            var kinds = (TokenType[]) Enum.GetValues(typeof(TokenType));
            foreach( var kind in kinds )
            {
                if( GetBinaryOperatorPrecedence( kind ) > 0 )
                    yield return kind;
            }
        }

    }
}
