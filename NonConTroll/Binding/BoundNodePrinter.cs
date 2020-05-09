using System;
using System.CodeDom.Compiler;
using System.Collections.Immutable;
using System.IO;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;
using NonConTroll.CodeAnalysis.IO;

namespace NonConTroll.CodeAnalysis.Binding
{
    public static class BoundNodePrinter
    {
        public static void WriteTo( this BoundNode node , TextWriter writer )
        {
            if( writer is IndentedTextWriter iw )
            {
                WriteTo( node , iw );
            }
            else
            {
                WriteTo( node , new IndentedTextWriter( writer ) );
            }
        }

        public static void WriteTo( this BoundNode boundNode , IndentedTextWriter writer )
        {
            switch( boundNode )
            {
                case BoundBlockStatement node:           WriteBlockStatement( node ,           writer ); break;
                case BoundVariableDeclaration node:      WriteVariableDeclaration( node ,      writer ); break;
                case BoundIfStatement node:              WriteIfStatement( node ,              writer ); break;
                case BoundWhileStatement node:           WriteWhileStatement( node ,           writer ); break;
                case BoundDoWhileStatement node:         WriteDoWhileStatement( node ,         writer ); break;
                case BoundForStatement node:             WriteForStatement( node ,             writer ); break;
                case BoundLabelStatement node:           WriteLabelStatement( node ,           writer ); break;
                case BoundGotoStatement node:            WriteGotoStatement( node ,            writer ); break;
                case BoundConditionalGotoStatement node: WriteConditionalGotoStatement( node , writer ); break;
                case BoundDeferStatement node:           WriteDeferStatement( node ,           writer ); break;
                case BoundReturnStatement node:          WriteReturnStatement( node ,          writer ); break;
                case BoundExpressionStatement node:      WriteExpressionStatement( node ,      writer ); break;
                case BoundErrorExpression node:          WriteErrorExpression( node ,          writer ); break;
                case BoundLiteralExpression node:        WriteLiteralExpression( node ,        writer ); break;
                case BoundVariableExpression node:       WriteVariableExpression( node ,       writer ); break;
                case BoundAssignmentExpression node:     WriteAssignmentExpression( node ,     writer ); break;
                case BoundUnaryExpression node:          WriteUnaryExpression( node ,          writer ); break;
                case BoundBinaryExpression node:         WriteBinaryExpression( node ,         writer ); break;
                case BoundCallExpression node:           WriteCallExpression( node ,           writer ); break;
                case BoundConversionExpression node:     WriteConversionExpression( node ,     writer ); break;
                case BoundMatchStatement node:           WriteMatchStatement( node ,          writer ); break;
                case BoundMatchExpression node:          WriteMatchExpression( node ,          writer ); break;
                case BoundPatternSectionStatement node:  WritePatternSectionStatement( node , writer ); break;
                case BoundPatternSectionExpression node: WritePatternSectionExpression( node , writer ); break;
                case BoundConstantPattern node:          WriteConstantPattern( node ,          writer ); break;
                case BoundInfixPattern node:             WriteInfixPattern( node ,             writer ); break;
                case BoundMatchAnyPattern node:          WriteMatchAnyPattern( node ,          writer ); break;
                default:
                    throw new Exception( $"Unexpected node {boundNode.Kind}" );
            }
        }

        private static void WriteMatchExpression( BoundMatchExpression node , IndentedTextWriter writer )
        {
            writer.Write( SyntaxKind.MatchKeyword );
            writer.WriteSpace();
            node.Expression.WriteTo( writer );
            writer.WriteLine();
            writer.Write( SyntaxKind.OpenBraceToken );
            writer.Indent++;

            foreach( var patternSection in node.PatternSections )
            {
                patternSection.WriteTo( writer );
            }

            writer.Indent--;
            writer.Write( SyntaxKind.CloseBraceToken );
        }

        private static void WriteMatchStatement( BoundMatchStatement node , IndentedTextWriter writer )
        {
            writer.Write( SyntaxKind.MatchKeyword );
            writer.WriteSpace();
            node.Expression.WriteTo( writer );
            writer.WriteLine();
            writer.Write( SyntaxKind.OpenBraceToken );
            writer.Indent++;

            foreach( var patternSection in node.PatternSections )
            {
                patternSection.WriteTo( writer );
            }

            writer.Indent--;
            writer.Write( SyntaxKind.CloseBraceToken );
        }

        private static void WritePatternSectionExpression( BoundPatternSectionExpression node , IndentedTextWriter writer )
        {
            WritePatterns( node.Patterns , writer );

            writer.WritePunctuation( SyntaxKind.EqGtToken );

            node.Expression.WriteTo( writer );
        }

        private static void WritePatternSectionStatement( BoundPatternSectionStatement node , IndentedTextWriter writer )
        {
            WritePatterns( node.Patterns , writer );

            writer.WritePunctuation( SyntaxKind.EqGtToken );

            node.Statement.WriteTo( writer );
        }

        private static void WritePatterns( ImmutableArray<BoundPattern> patterns , IndentedTextWriter writer )
        {
            var isFirst = true;

            foreach( var pattern in patterns )
            {
                if( isFirst )
                {
                    isFirst = false;
                }
                else
                {
                    writer.WritePunctuation( SyntaxKind.CommaToken );
                    writer.WriteSpace();
                }

                pattern.WriteTo( writer );
            }
        }

        private static void WriteConstantPattern( BoundConstantPattern node , IndentedTextWriter writer )
        {
            node.Expression.WriteTo( writer );
        }

        private static void WriteMatchAnyPattern( BoundMatchAnyPattern node , IndentedTextWriter writer )
        {
            writer.WritePunctuation( SyntaxKind.UnderscoreToken );
        }

        private static void WriteInfixPattern( BoundInfixPattern node , IndentedTextWriter writer )
        {
            node.InfixFunction.WriteTo( writer );
            writer.WriteSpace();
            node.Expression.WriteTo( writer );
        }

        private static void WriteNestedStatement( this IndentedTextWriter writer , BoundStatement node )
        {
            var needsIndentation = !(node is BoundBlockStatement);

            if( needsIndentation )
            {
                writer.Indent++;
            }

            node.WriteTo( writer );

            if( needsIndentation )
            {
                writer.Indent--;
            }
        }

        private static void WriteNestedExpression( this IndentedTextWriter writer , int parentPrecedence , BoundExpression expression )
        {
            if( expression is BoundUnaryExpression unary )
            {
                writer.WriteNestedExpression( parentPrecedence , unary.Op.TkType.GetUnaryOperatorPrecedence() , unary );
            }
            else if( expression is BoundBinaryExpression binary )
            {
                writer.WriteNestedExpression( parentPrecedence , binary.Operator.TkType.GetBinaryOperatorPrecedence() , binary );
            }
            else
            {
                expression.WriteTo( writer );
            }
        }

        private static void WriteNestedExpression( this IndentedTextWriter writer , int parentPrecedence , int currentPrecedence , BoundExpression expression )
        {
            var needsParenthesis = parentPrecedence >= currentPrecedence;

            if( needsParenthesis )
            {
                writer.WritePunctuation( SyntaxKind.OpenParenToken );
            }

            expression.WriteTo( writer );

            if( needsParenthesis )
            {
                writer.WritePunctuation( SyntaxKind.CloseParenToken );
            }
        }

        private static void WriteBlockStatement( BoundBlockStatement node , IndentedTextWriter writer )
        {
            writer.WritePunctuation( SyntaxKind.OpenBraceToken );
            writer.WriteLine();
            writer.Indent++;

            foreach( var s in node.Statements )
            {
                s.WriteTo( writer );
            }

            writer.Indent--;
            writer.WritePunctuation( SyntaxKind.CloseBraceToken );
            writer.WriteLine();
        }

        private static void WriteVariableDeclaration( BoundVariableDeclaration node , IndentedTextWriter writer )
        {
            writer.Write( node.Variable.IsReadOnly ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword );
            writer.WriteSpace();
            writer.WriteIdentifier( node.Variable.Name );
            writer.WriteSpace();
            writer.WritePunctuation( SyntaxKind.EqToken );
            writer.WriteSpace();
            node.Initializer.WriteTo( writer );
            writer.WriteLine();
        }

        private static void WriteIfStatement( BoundIfStatement node , IndentedTextWriter writer )
        {
            writer.Write( SyntaxKind.IfKeyword );
            writer.WriteSpace();
            node.Condition.WriteTo( writer );
            writer.WriteLine();
            writer.WriteNestedStatement( node.ThenStatement );

            if( node.ElseStatement != null )
            {
                writer.Write( SyntaxKind.ElseKeyword );
                writer.WriteLine();
                writer.WriteNestedStatement( node.ElseStatement );
            }
        }

        private static void WriteWhileStatement( BoundWhileStatement node , IndentedTextWriter writer )
        {
            writer.Write( SyntaxKind.WhileKeyword );
            writer.WriteSpace();
            node.Condition.WriteTo( writer );
            writer.WriteLine();
            writer.WriteNestedStatement( node.Body );
        }

        private static void WriteDoWhileStatement( BoundDoWhileStatement node , IndentedTextWriter writer )
        {
            writer.Write( SyntaxKind.DoKeyword );
            writer.WriteLine();
            writer.WriteNestedStatement( node.Body );
            writer.Write( SyntaxKind.WhileKeyword );
            writer.WriteSpace();
            node.Condition.WriteTo( writer );
            writer.WriteLine();
        }

        private static void WriteForStatement( BoundForStatement node , IndentedTextWriter writer )
        {
            writer.Write( SyntaxKind.ForKeyword );
            writer.WriteSpace();
            writer.WriteIdentifier( node.Variable.Name );
            writer.WriteSpace();
            writer.WritePunctuation( SyntaxKind.EqToken );
            writer.WriteSpace();
            node.LowerBound.WriteTo( writer );
            writer.WriteSpace();
            writer.Write( SyntaxKind.To );
            writer.WriteSpace();
            node.UpperBound.WriteTo( writer );
            writer.WriteLine();
            writer.WriteNestedStatement( node.Body );
        }

        private static void WriteLabelStatement( BoundLabelStatement node , IndentedTextWriter writer )
        {
            var unindent = writer.Indent > 0;

            if( unindent )
            {
                writer.Indent--;
            }

            writer.WritePunctuation( node.Label.Name );
            writer.WritePunctuation( SyntaxKind.ColonToken );
            writer.WriteLine();

            if( unindent )
            {
                writer.Indent++;
            }
        }

        private static void WriteGotoStatement( BoundGotoStatement node , IndentedTextWriter writer )
        {
            writer.Write( "goto" );
            writer.WriteSpace();
            writer.WriteIdentifier( node.Label.Name );
            writer.WriteLine();
        }

        private static void WriteConditionalGotoStatement( BoundConditionalGotoStatement node , IndentedTextWriter writer )
        {
            writer.Write( "goto" );
            writer.WriteSpace();
            writer.WriteIdentifier( node.Label.Name );
            writer.WriteSpace();
            writer.Write( node.JumpIfTrue ? "if" : "unless" );
            writer.WriteSpace();
            node.Condition.WriteTo( writer );
            writer.WriteLine();
        }

        private static void WriteDeferStatement( BoundDeferStatement node , IndentedTextWriter writer )
        {
            writer.Write( SyntaxKind.DeferKeyword );
            writer.WriteSpace();
            node.Expression.WriteTo( writer );
            writer.WriteLine();
        }

        private static void WriteReturnStatement( BoundReturnStatement node , IndentedTextWriter writer )
        {
            writer.Write( SyntaxKind.ReturnKeyword );

            if( node.Expression != null )
            {
                writer.WriteSpace();
                node.Expression.WriteTo( writer );
            }

            writer.WriteLine();
        }

        private static void WriteExpressionStatement( BoundExpressionStatement node , IndentedTextWriter writer )
        {
            node.Expression.WriteTo( writer );
            writer.WriteLine();
        }

        private static void WriteErrorExpression( BoundErrorExpression node , IndentedTextWriter writer )
        {
            writer.Write( "?" );
        }

        private static void WriteLiteralExpression( BoundLiteralExpression node , IndentedTextWriter writer )
        {
            var value = node.Value.ToString()!;

            if( node.Type == TypeSymbol.Bool )
            {
                writer.Write( (bool)node.Value ? SyntaxKind.TrueKeywordLiteral : SyntaxKind.FalseKeywordLiteral );
            }
            else if( node.Type == TypeSymbol.Int )
            {
                writer.WriteNumber( value );
            }
            else if( node.Type == TypeSymbol.String )
            {
                value = "\"" + value.Replace( "\"" , "\"\"" ) + "\"";
                writer.WriteString( value );
            }
            else
            {
                throw new Exception( $"Unexpected type {node.Type}" );
            }
        }

        private static void WriteVariableExpression( BoundVariableExpression node , IndentedTextWriter writer )
        {
            writer.WriteIdentifier( node.Variable.Name );
        }

        private static void WriteAssignmentExpression( BoundAssignmentExpression node , IndentedTextWriter writer )
        {
            writer.WriteIdentifier( node.Variable.Name );
            writer.WriteSpace();
            writer.WritePunctuation( SyntaxKind.EqToken );
            writer.WriteSpace();
            node.Expression.WriteTo( writer );
        }

        private static void WriteUnaryExpression( BoundUnaryExpression node , IndentedTextWriter writer )
        {
            var precedence = node.Op.TkType.GetUnaryOperatorPrecedence();

            writer.WritePunctuation( node.Op.TkType );
            writer.WriteNestedExpression( precedence , node.Operand );
        }

        private static void WriteBinaryExpression( BoundBinaryExpression node , IndentedTextWriter writer )
        {
            var precedence = node.Operator.TkType.GetBinaryOperatorPrecedence();

            writer.WriteNestedExpression( precedence , node.Lhs );
            writer.WriteSpace();
            writer.WritePunctuation( node.Operator.TkType );
            writer.WriteSpace();
            writer.WriteNestedExpression( precedence , node.Rhs );
        }

        private static void WriteCallExpression( BoundCallExpression node , IndentedTextWriter writer )
        {
            writer.WriteIdentifier( node.Function.Name );
            writer.WritePunctuation( SyntaxKind.OpenParenToken );

            var isFirst = true;

            foreach( var argument in node.Arguments )
            {
                if( isFirst )
                {
                    isFirst = false;
                }
                else
                {
                    writer.WritePunctuation( SyntaxKind.CommaToken );
                    writer.WriteSpace();
                }

                argument.WriteTo( writer );
            }

            writer.WritePunctuation( SyntaxKind.CloseParenToken );
        }

        private static void WriteConversionExpression( BoundConversionExpression node , IndentedTextWriter writer )
        {
            writer.WriteIdentifier( node.Type.Name );
            writer.WritePunctuation( SyntaxKind.OpenParenToken );
            node.Expression.WriteTo( writer );
            writer.WritePunctuation( SyntaxKind.CloseParenToken );
        }
    }
}
