using System;
using System.Collections.Generic;
using System.Linq;

namespace NonConTroll.CodeAnalysis.Syntax
{

    public static class SyntaxInfo
    {
        public static SyntaxKind? GetSyntaxKind( string text )
            => text switch {
                "func"      => SyntaxKind.FuncKeyword ,
                "fn"        => SyntaxKind.FnKeyword ,
                "return"    => SyntaxKind.ReturnKeyword ,
                "ret"       => SyntaxKind.RetKeyword ,
                "class"     => SyntaxKind.ClassKeyword ,
                "struct"    => SyntaxKind.Struct ,
                "mixin"     => SyntaxKind.Mixin ,
                "abstract"  => SyntaxKind.Abstract ,
                "meta"      => SyntaxKind.Meta ,
                "template"  => SyntaxKind.Template ,
                "import"    => SyntaxKind.Import ,
                "export"    => SyntaxKind.Export ,
                "implicit"  => SyntaxKind.Implicit ,
                "explicit"  => SyntaxKind.Explicit ,
                "internal"  => SyntaxKind.Internal ,
                "external"  => SyntaxKind.External ,
                "public"    => SyntaxKind.Public ,
                "pub"       => SyntaxKind.Pub ,
                "protected" => SyntaxKind.Protected ,
                "private"   => SyntaxKind.Private ,
                "if"        => SyntaxKind.IfKeyword ,
                "then"      => SyntaxKind.Then ,
                "when"      => SyntaxKind.When ,
                "else"      => SyntaxKind.ElseKeyword ,
                "elif"      => SyntaxKind.Elif ,
                "while"     => SyntaxKind.WhileKeyword ,
                "loop"      => SyntaxKind.Loop ,
                "for"       => SyntaxKind.ForKeyword ,
                "foreach"   => SyntaxKind.Foreach ,
                "match"     => SyntaxKind.MatchKeyword ,
                "switch"    => SyntaxKind.Switch ,
                "case"      => SyntaxKind.Case ,
                "default"   => SyntaxKind.Default ,
                "defer"     => SyntaxKind.DeferKeyword ,
                "delete"    => SyntaxKind.Delete ,
                "except"    => SyntaxKind.Except ,
                "try"       => SyntaxKind.Try ,
                "catch"     => SyntaxKind.Catch ,
                "expect"    => SyntaxKind.Expect ,
                "ensure"    => SyntaxKind.Ensure ,
                "in"        => SyntaxKind.In ,
                "out"       => SyntaxKind.Out ,
                "ref"       => SyntaxKind.RefKeyword ,
                "val"       => SyntaxKind.Val ,
                "ptr"       => SyntaxKind.PtrKeyword ,
                "var"       => SyntaxKind.VarKeyword ,
                "let"       => SyntaxKind.LetKeyword ,
                "const"     => SyntaxKind.Const ,
                "readonly"  => SyntaxKind.Readonly ,
                "volatile"  => SyntaxKind.Volatile ,
                "enum"      => SyntaxKind.Enum ,
                "union"     => SyntaxKind.Union ,
                "is"        => SyntaxKind.Is ,
                "as"        => SyntaxKind.As ,
                "cast"      => SyntaxKind.Cast ,
                "operator"  => SyntaxKind.Operator ,
                "namespace" => SyntaxKind.Namespace ,
                "package"   => SyntaxKind.Package ,
                "module"    => SyntaxKind.Module ,
                "break"     => SyntaxKind.BreakKeyword ,
                "continue"  => SyntaxKind.ContinueKeyword ,
                "do"        => SyntaxKind.DoKeyword ,
                "to"        => SyntaxKind.To ,

                "true"      => SyntaxKind.TrueKeywordLiteral ,
                "false"     => SyntaxKind.FalseKeywordLiteral ,
                "null"      => SyntaxKind.NullKeywordLiteral ,
                "undefined" => SyntaxKind.UndefinedKeywordLiteral ,

                _ => null,
            };

        public static string? GetName( this SyntaxKind kind )
            => kind switch {
                SyntaxKind.FuncKeyword     => "Func" ,
                SyntaxKind.FnKeyword       => "fn" ,
                SyntaxKind.ReturnKeyword   => "return" ,
                SyntaxKind.RetKeyword      => "ret" ,
                SyntaxKind.ClassKeyword           => "class" ,
                SyntaxKind.Struct          => "struct" ,
                SyntaxKind.Mixin           => "mixin" ,
                SyntaxKind.Abstract        => "abstract" ,
                SyntaxKind.Meta            => "meta" ,
                SyntaxKind.Template        => "template" ,
                SyntaxKind.Import          => "import" ,
                SyntaxKind.Export          => "export" ,
                SyntaxKind.Implicit        => "implicit" ,
                SyntaxKind.Explicit        => "explicit" ,
                SyntaxKind.Internal        => "internal" ,
                SyntaxKind.External        => "external" ,
                SyntaxKind.Public          => "public" ,
                SyntaxKind.Pub             => "pub" ,
                SyntaxKind.Protected       => "protected" ,
                SyntaxKind.Private         => "private" ,
                SyntaxKind.IfKeyword       => "if" ,
                SyntaxKind.Then            => "then" ,
                SyntaxKind.When            => "when" ,
                SyntaxKind.ElseKeyword     => "else" ,
                SyntaxKind.Elif            => "elif" ,
                SyntaxKind.WhileKeyword    => "while" ,
                SyntaxKind.Loop            => "loop" ,
                SyntaxKind.ForKeyword      => "for" ,
                SyntaxKind.Foreach         => "foreach" ,
                SyntaxKind.MatchKeyword    => "match" ,
                SyntaxKind.Switch          => "switch" ,
                SyntaxKind.Case            => "case" ,
                SyntaxKind.Default         => "default" ,
                SyntaxKind.DeferKeyword    => "defer" ,
                SyntaxKind.Delete          => "delete" ,
                SyntaxKind.Except          => "except" ,
                SyntaxKind.Try             => "try" ,
                SyntaxKind.Catch           => "catch" ,
                SyntaxKind.Expect          => "expect" ,
                SyntaxKind.Ensure          => "ensure" ,
                SyntaxKind.In              => "in" ,
                SyntaxKind.Out             => "out" ,
                SyntaxKind.RefKeyword      => "ref" ,
                SyntaxKind.Val             => "val" ,
                SyntaxKind.PtrKeyword      => "ptr" ,
                SyntaxKind.VarKeyword      => "var" ,
                SyntaxKind.LetKeyword      => "let" ,
                SyntaxKind.Const           => "const" ,
                SyntaxKind.Readonly        => "readonly" ,
                SyntaxKind.Volatile        => "volatile" ,
                SyntaxKind.Enum            => "enum" ,
                SyntaxKind.Union           => "union" ,
                SyntaxKind.Is              => "is" ,
                SyntaxKind.As              => "as" ,
                SyntaxKind.Cast            => "cast" ,
                SyntaxKind.Operator        => "operator" ,
                SyntaxKind.Namespace       => "namespace" ,
                SyntaxKind.Package         => "package" ,
                SyntaxKind.Module          => "module" ,
                SyntaxKind.BreakKeyword    => "break" ,
                SyntaxKind.ContinueKeyword => "continue" ,
                SyntaxKind.DoKeyword       => "do" ,
                SyntaxKind.To              => "to" ,

                SyntaxKind.TrueKeywordLiteral      => "true" ,
                SyntaxKind.FalseKeywordLiteral     => "false" ,
                SyntaxKind.NullKeywordLiteral      => "null" ,
                SyntaxKind.UndefinedKeywordLiteral => "undefined" ,

                SyntaxKind.DotToken          => "." ,
                SyntaxKind.DotDotToken       => ".." ,
                SyntaxKind.DotDotDotToken    => "..." ,
                SyntaxKind.ColonToken        => ":" ,
                SyntaxKind.CommaToken        => "," ,
                SyntaxKind.SemicolonToken    => ";" ,
                SyntaxKind.OpenParenToken    => "(" ,
                SyntaxKind.CloseParenToken   => ")" ,
                SyntaxKind.OpenBraceToken    => "{" ,
                SyntaxKind.CloseBraceToken   => "}" ,
                SyntaxKind.OpenBracketToken  => "[" ,
                SyntaxKind.CloseBracketToken => "]" ,
                SyntaxKind.MinusToken        => "-" ,
                SyntaxKind.PlusToken         => "+" ,
                SyntaxKind.StarToken         => "*" ,
                SyntaxKind.SlashToken        => "/" ,
                SyntaxKind.EqToken           => "=" ,
                SyntaxKind.EqEqToken         => "==" ,
                SyntaxKind.ExmToken          => "!" ,
                SyntaxKind.QmToken           => "?" ,
                SyntaxKind.ExmEqToken        => "!=" ,
                SyntaxKind.PipeToken         => "|" ,
                SyntaxKind.PipePipeToken     => "||" ,
                SyntaxKind.AndToken          => "&" ,
                SyntaxKind.AndAndToken       => "&&" ,
                SyntaxKind.LtToken           => "<" ,
                SyntaxKind.LtEqToken         => "<=" ,
                SyntaxKind.GtToken           => ">" ,
                SyntaxKind.GtEqToken         => ">=" ,
                SyntaxKind.MinusGtToken      => "->" ,
                SyntaxKind.LtMinusToken      => "<-" ,
                SyntaxKind.EqGtToken         => "=>" ,
                SyntaxKind.PlusEq            => "+=" ,
                SyntaxKind.MinusEqToken      => "-=" ,
                SyntaxKind.StarEqToken       => "*=" ,
                SyntaxKind.SlashEqToken      => "/=" ,
                SyntaxKind.SingleQuoteToken  => "'" ,
                SyntaxKind.DoubleQuoteToken  => "\"" ,
                SyntaxKind.PercentToken      => "%" ,
                SyntaxKind.DollarToken       => "$" ,
                SyntaxKind.CaretToken        => "^" ,
                SyntaxKind.TildeToken        => "~" ,
                SyntaxKind.HashtagToken      => "#" ,
                SyntaxKind.UnderscoreToken   => "_" ,
                SyntaxKind.BackSlash         => "\\" ,
                SyntaxKind.BackTickToken     => "`" ,
                SyntaxKind.FrontTickToken    => "´" ,
                SyntaxKind.AtToken           => "@" ,

                _ => null,
            };



        public static int GetUnaryOperatorPrecedence( this SyntaxKind tokenType )
        {
            switch( tokenType )
            {
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                case SyntaxKind.ExmToken:
                    return 8;
                // case TokenType.Identifier: // unary infix function calls
                //     return 7;
                default:
                    return 0;
            }
        }

        public static int GetBinaryOperatorPrecedence( this SyntaxKind tokenType )
        {
            switch( tokenType )
            {
                case SyntaxKind.StarToken:
                case SyntaxKind.SlashToken:
                    return 6;
                case SyntaxKind.PlusToken:
                case SyntaxKind.MinusToken:
                    return 5;
                case SyntaxKind.EqEqToken:
                case SyntaxKind.ExmEqToken:
                case SyntaxKind.LtToken:
                case SyntaxKind.LtEqToken:
                case SyntaxKind.GtToken:
                case SyntaxKind.GtEqToken:
                    return 4;
                case SyntaxKind.AndAndToken:
                    return 3;
                case SyntaxKind.PipePipeToken:
                    return 2;
                // case TokenType.Identifier: // binary infix function calls
                //     return 1;
                default:
                    return 0;
            }
        }
    }
}
