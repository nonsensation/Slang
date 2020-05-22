using System.Collections.Generic;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.Tests.CodeAnalysis.Syntax;
using Xunit;

namespace NonConTroll.Tests.CodeAnalysis
{
    public class ParserTests
    {
        [Theory]
        [MemberData( nameof( GetBinaryOperatorPairsData ) )]
        public void Parser_BinaryExpression_HonorsPrecedences( SyntaxKind op1 , SyntaxKind op2 )
        {
            var op1Precedence = SyntaxInfo.GetBinaryOperatorPrecedence( op1 );
            var op2Precedence = SyntaxInfo.GetBinaryOperatorPrecedence( op2 );
            var op1Text = SyntaxInfo.GetText( op1 );
            var op2Text = SyntaxInfo.GetText( op2 );
            var text = $"a {op1Text} b {op2Text} c";
            var expression = ParseExpression( text );

            using var e = new AssertingEnumerator( expression );

            if( op1Precedence >= op2Precedence )
            {
                //     op2
                //    /   \
                //   op1   c
                //  /   \
                // a     b


                e.AssertNode( SyntaxKind.BinaryExpression );
                e.AssertNode( SyntaxKind.BinaryExpression );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "a" );
                e.AssertToken( op1 , op1Text );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "b" );
                e.AssertToken( op2 , op2Text );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "c" );
            }
            else
            {
                //   op1
                //  /   \
                // a    op2
                //     /   \
                //    b     c

                e.AssertNode( SyntaxKind.BinaryExpression );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "a" );
                e.AssertToken( op1 , op1Text );
                e.AssertNode( SyntaxKind.BinaryExpression );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "b" );
                e.AssertToken( op2 , op2Text );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "c" );
            }
        }

        [Theory]
        [MemberData( nameof( GetUnaryOperatorPairsData ) )]
        public void Parser_UnaryExpression_HonorsPrecedences( SyntaxKind unaryKind , SyntaxKind binaryKind )
        {
            var unaryPrecedence = SyntaxInfo.GetUnaryOperatorPrecedence( unaryKind );
            var binaryPrecedence = SyntaxInfo.GetBinaryOperatorPrecedence( binaryKind );
            var unaryText = SyntaxInfo.GetText( unaryKind );
            var binaryText = SyntaxInfo.GetText( binaryKind );
            var text = $"{unaryText} a {binaryText} b";
            var expression = ParseExpression( text );

            using var e = new AssertingEnumerator( expression );

            if( unaryPrecedence >= binaryPrecedence )
            {
                //   binary
                //   /    \
                // unary   b
                //   |
                //   a

                e.AssertNode( SyntaxKind.BinaryExpression );
                e.AssertNode( SyntaxKind.UnaryExpression );
                e.AssertToken( unaryKind , unaryText );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "a" );
                e.AssertToken( binaryKind , binaryText );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "b" );
            }
            else
            {
                //  unary
                //    |
                //  binary
                //  /   \
                // a     b

                e.AssertNode( SyntaxKind.UnaryExpression );
                e.AssertToken( unaryKind , unaryText );
                e.AssertNode( SyntaxKind.BinaryExpression );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "a" );
                e.AssertToken( binaryKind , binaryText );
                e.AssertNode( SyntaxKind.NameExpression );
                e.AssertToken( SyntaxKind.Identifier , "b" );
            }
        }

        private static ExpressionSyntax ParseExpression( string text )
        {
            var syntaxTree = SyntaxTree.Parse( text );
            var root = syntaxTree.Root;
            var member = Assert.Single( root.Members );
            var globalStatement = Assert.IsType<GlobalStatementSyntax>( member );

            return Assert.IsType<ExpressionStatementSyntax>( globalStatement.Statement ).Expression;
        }

        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            foreach( var op1 in SyntaxInfo.GetBinaryOperatorKinds() )
            {
                foreach( var op2 in SyntaxInfo.GetBinaryOperatorKinds() )
                {
                    yield return new object[] { op1 , op2 };
                }
            }
        }

        public static IEnumerable<object[]> GetUnaryOperatorPairsData()
        {
            foreach( var unary in SyntaxInfo.GetUnaryOperatorKinds() )
            {
                foreach( var binary in SyntaxInfo.GetBinaryOperatorKinds() )
                {
                    yield return new object[] { unary , binary };
                }
            }
        }
    }

}
