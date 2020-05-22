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
                // "fn"        => SyntaxKind.FnKeyword ,
                "return"    => SyntaxKind.ReturnKeyword ,
                // "ret"       => SyntaxKind.RetKeyword ,
                // "class"     => SyntaxKind.ClassKeyword ,
                // "struct"    => SyntaxKind.StructKeyword ,
                // "mixin"     => SyntaxKind.MixinKeyword ,
                // "abstract"  => SyntaxKind.AbstractKeyword ,
                // "meta"      => SyntaxKind.MetaKeyword ,
                // "template"  => SyntaxKind.TemplateKeyword ,
                // "import"    => SyntaxKind.ImportKeyword ,
                // "export"    => SyntaxKind.ExportKeyword ,
                // "implicit"  => SyntaxKind.ImplicitKeyword ,
                // "explicit"  => SyntaxKind.ExplicitKeyword ,
                // "internal"  => SyntaxKind.InternalKeyword ,
                // "external"  => SyntaxKind.ExternalKeyword ,
                // "public"    => SyntaxKind.PublicKeyword ,
                // "pub"       => SyntaxKind.PubKeyword ,
                // "protected" => SyntaxKind.ProtectedKeyword ,
                // "private"   => SyntaxKind.PrivateKeyword ,
                "if"        => SyntaxKind.IfKeyword ,
                // "then"      => SyntaxKind.ThenKeyword ,
                // "when"      => SyntaxKind.WhenKeyword ,
                "else"      => SyntaxKind.ElseKeyword ,
                // "elif"      => SyntaxKind.ElifKeyword ,
                "while"     => SyntaxKind.WhileKeyword ,
                // "loop"      => SyntaxKind.LoopKeyword ,
                "for"       => SyntaxKind.ForKeyword ,
                // "foreach"   => SyntaxKind.ForeachKeyword ,
                "match"     => SyntaxKind.MatchKeyword ,
                // "switch"    => SyntaxKind.SwitchKeyword ,
                // "case"      => SyntaxKind.CaseKeyword ,
                // "default"   => SyntaxKind.DefaultKeyword ,
                "defer"     => SyntaxKind.DeferKeyword ,
                // "delete"    => SyntaxKind.DeleteKeyword ,
                // "except"    => SyntaxKind.ExceptKeyword ,
                // "try"       => SyntaxKind.TryKeyword ,
                // "catch"     => SyntaxKind.CatchKeyword ,
                // "expect"    => SyntaxKind.ExpectKeyword ,
                // "ensure"    => SyntaxKind.EnsureKeyword ,
                "in"        => SyntaxKind.InKeyword ,
                // "out"       => SyntaxKind.OutKeyword ,
                // "ref"       => SyntaxKind.RefKeyword ,
                // "val"       => SyntaxKind.ValKeyword ,
                // "ptr"       => SyntaxKind.PtrKeyword ,
                "var"       => SyntaxKind.VarKeyword ,
                "let"       => SyntaxKind.LetKeyword ,
                // "const"     => SyntaxKind.ConstKeyword ,
                // "readonly"  => SyntaxKind.ReadonlyKeyword ,
                // "volatile"  => SyntaxKind.VolatileKeyword ,
                // "enum"      => SyntaxKind.EnumKeyword ,
                // "union"     => SyntaxKind.UnionKeyword ,
                // "is"        => SyntaxKind.IsKeyword ,
                // "as"        => SyntaxKind.AsKeyword ,
                // "cast"      => SyntaxKind.CastKeyword ,
                // "operator"  => SyntaxKind.OperatorKeyword ,
                // "namespace" => SyntaxKind.NamespaceKeyword ,
                // "package"   => SyntaxKind.PackageKeyword ,
                // "module"    => SyntaxKind.ModuleKeyword ,
                "break"     => SyntaxKind.BreakKeyword ,
                "continue"  => SyntaxKind.ContinueKeyword ,
                // "do"        => SyntaxKind.DoKeyword ,
                // "to"        => SyntaxKind.ToKeyword ,

                "true"      => SyntaxKind.TrueKeywordLiteral ,
                "false"     => SyntaxKind.FalseKeywordLiteral ,
                // "null"      => SyntaxKind.NullKeywordLiteral ,
                // "undefined" => SyntaxKind.UndefinedKeywordLiteral ,

                _ => null,
            };

        public static string? GetText( this SyntaxKind kind )
            => kind switch {
                SyntaxKind.FuncKeyword      => "func"      ,
                // SyntaxKind.FnKeyword        => "fn"        ,
                SyntaxKind.ReturnKeyword    => "return"    ,
                // SyntaxKind.RetKeyword       => "ret"       ,
                // SyntaxKind.ClassKeyword     => "class"     ,
                // SyntaxKind.StructKeyword    => "struct"    ,
                // SyntaxKind.MixinKeyword     => "mixin"     ,
                // SyntaxKind.AbstractKeyword  => "abstract"  ,
                // SyntaxKind.MetaKeyword      => "meta"      ,
                // SyntaxKind.TemplateKeyword  => "template"  ,
                // SyntaxKind.ImportKeyword    => "import"    ,
                // SyntaxKind.ExportKeyword    => "export"    ,
                // SyntaxKind.ImplicitKeyword  => "implicit"  ,
                // SyntaxKind.ExplicitKeyword  => "explicit"  ,
                // SyntaxKind.InternalKeyword  => "internal"  ,
                // SyntaxKind.ExternalKeyword  => "external"  ,
                // SyntaxKind.PublicKeyword    => "public"    ,
                // SyntaxKind.PubKeyword       => "pub"       ,
                // SyntaxKind.ProtectedKeyword => "protected" ,
                // SyntaxKind.PrivateKeyword   => "private"   ,
                SyntaxKind.IfKeyword        => "if"        ,
                // SyntaxKind.ThenKeyword      => "then"      ,
                // SyntaxKind.WhenKeyword      => "when"      ,
                SyntaxKind.ElseKeyword      => "else"      ,
                // SyntaxKind.ElifKeyword      => "elif"      ,
                SyntaxKind.WhileKeyword     => "while"     ,
                // SyntaxKind.LoopKeyword      => "loop"      ,
                SyntaxKind.ForKeyword       => "for"       ,
                // SyntaxKind.ForeachKeyword   => "foreach"   ,
                SyntaxKind.MatchKeyword     => "match"     ,
                // SyntaxKind.SwitchKeyword    => "switch"    ,
                // SyntaxKind.CaseKeyword      => "case"      ,
                // SyntaxKind.DefaultKeyword   => "default"   ,
                SyntaxKind.DeferKeyword     => "defer"     ,
                // SyntaxKind.DeleteKeyword    => "delete"    ,
                // SyntaxKind.ExceptKeyword    => "except"    ,
                // SyntaxKind.TryKeyword       => "try"       ,
                // SyntaxKind.CatchKeyword     => "catch"     ,
                // SyntaxKind.ExpectKeyword    => "expect"    ,
                // SyntaxKind.EnsureKeyword    => "ensure"    ,
                SyntaxKind.InKeyword        => "in"        ,
                // SyntaxKind.OutKeyword       => "out"       ,
                // SyntaxKind.RefKeyword       => "ref"       ,
                // SyntaxKind.ValKeyword       => "val"       ,
                // SyntaxKind.PtrKeyword       => "ptr"       ,
                SyntaxKind.VarKeyword       => "var"       ,
                SyntaxKind.LetKeyword       => "let"       ,
                // SyntaxKind.ConstKeyword     => "const"     ,
                // SyntaxKind.ReadonlyKeyword  => "readonly"  ,
                // SyntaxKind.VolatileKeyword  => "volatile"  ,
                // SyntaxKind.EnumKeyword      => "enum"      ,
                // SyntaxKind.UnionKeyword     => "union"     ,
                // SyntaxKind.IsKeyword        => "is"        ,
                // SyntaxKind.AsKeyword        => "as"        ,
                // SyntaxKind.CastKeyword      => "cast"      ,
                // SyntaxKind.OperatorKeyword  => "operator"  ,
                // SyntaxKind.NamespaceKeyword => "namespace" ,
                // SyntaxKind.PackageKeyword   => "package"   ,
                // SyntaxKind.ModuleKeyword    => "module"    ,
                SyntaxKind.BreakKeyword     => "break"     ,
                SyntaxKind.ContinueKeyword  => "continue"  ,
                // SyntaxKind.DoKeyword        => "do"        ,
                // SyntaxKind.ToKeyword        => "to"        ,

                SyntaxKind.TrueKeywordLiteral      => "true"      ,
                SyntaxKind.FalseKeywordLiteral     => "false"     ,
                // SyntaxKind.NullKeywordLiteral      => "null"      ,
                // SyntaxKind.UndefinedKeywordLiteral => "undefined" ,

                // SyntaxKind.DotToken          => "."   ,
                // SyntaxKind.DotDotToken       => ".."  ,
                // SyntaxKind.DotDotDotToken    => "..." ,
                SyntaxKind.ColonToken        => ":"   ,
                SyntaxKind.CommaToken        => ","   ,
                // SyntaxKind.SemicolonToken    => ";"   ,
                SyntaxKind.OpenParenToken    => "("   ,
                SyntaxKind.CloseParenToken   => ")"   ,
                SyntaxKind.OpenBraceToken    => "{"   ,
                SyntaxKind.CloseBraceToken   => "}"   ,
                // SyntaxKind.OpenBracketToken  => "["   ,
                // SyntaxKind.CloseBracketToken => "]"   ,
                SyntaxKind.MinusToken        => "-"   ,
                SyntaxKind.PlusToken         => "+"   ,
                SyntaxKind.StarToken         => "*"   ,
                SyntaxKind.SlashToken        => "/"   ,
                SyntaxKind.EqToken           => "="   ,
                SyntaxKind.EqEqToken         => "=="  ,
                SyntaxKind.ExmToken          => "!"   ,
                // SyntaxKind.QmToken           => "?"   ,
                SyntaxKind.ExmEqToken        => "!="  ,
                // SyntaxKind.PipeToken         => "|"   ,
                SyntaxKind.PipePipeToken     => "||"  ,
                // SyntaxKind.AndToken          => "&"   ,
                SyntaxKind.AndAndToken       => "&&"  ,
                SyntaxKind.LtToken           => "<"   ,
                SyntaxKind.LtEqToken         => "<="  ,
                SyntaxKind.GtToken           => ">"   ,
                SyntaxKind.GtEqToken         => ">="  ,
                // SyntaxKind.MinusGtToken      => "->"  ,
                // SyntaxKind.LtMinusToken      => "<-"  ,
                SyntaxKind.EqGtToken         => "=>"  ,
                // SyntaxKind.PlusEq            => "+="  ,
                // SyntaxKind.MinusEqToken      => "-="  ,
                // SyntaxKind.StarEqToken       => "*="  ,
                // SyntaxKind.SlashEqToken      => "/="  ,
                // SyntaxKind.SingleQuoteToken  => "'"   ,
                // SyntaxKind.DoubleQuoteToken  => "\""  ,
                // SyntaxKind.PercentToken      => "%"   ,
                // SyntaxKind.DollarToken       => "$"   ,
                // SyntaxKind.CaretToken        => "^"   ,
                // SyntaxKind.TildeToken        => "~"   ,
                // SyntaxKind.HashtagToken      => "#"   ,
                SyntaxKind.UnderscoreToken   => "_"   ,
                // SyntaxKind.BackSlash         => "\\"  ,
                // SyntaxKind.BackTickToken     => "`"   ,
                // SyntaxKind.FrontTickToken    => "´"   ,
                // SyntaxKind.AtToken           => "@"   ,

                _ => null,
            };

        public static bool IsKeyword( this SyntaxKind kind )
            => kind.ToString().Contains( "Keyword" );

        public static bool IsPunctuation( this SyntaxKind kind )
            => kind.ToString().Contains( "Token" );


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
