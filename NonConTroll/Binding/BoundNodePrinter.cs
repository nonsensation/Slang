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
                WriteTo( node , iw );
            else
                WriteTo( node , new IndentedTextWriter( writer ) );
        }

        public static void WriteTo( this BoundNode node , IndentedTextWriter writer )
        {
            switch( node.Kind )
            {
                case BoundNodeKind.BlockStatement:
                    WriteBlockStatement( (BoundBlockStatement)node , writer );
                    break;
                case BoundNodeKind.VariableDeclaration:
                    WriteVariableDeclaration( (BoundVariableDeclaration)node , writer );
                    break;
                case BoundNodeKind.IfStatement:
                    WriteIfStatement( (BoundIfStatement)node , writer );
                    break;
                case BoundNodeKind.WhileStatement:
                    WriteWhileStatement( (BoundWhileStatement)node , writer );
                    break;
                case BoundNodeKind.DoWhileStatement:
                    WriteDoWhileStatement( (BoundDoWhileStatement)node , writer );
                    break;
                case BoundNodeKind.ForStatement:
                    WriteForStatement( (BoundForStatement)node , writer );
                    break;
                case BoundNodeKind.LabelStatement:
                    WriteLabelStatement( (BoundLabelStatement)node , writer );
                    break;
                case BoundNodeKind.GotoStatement:
                    WriteGotoStatement( (BoundGotoStatement)node , writer );
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    WriteConditionalGotoStatement( (BoundConditionalGotoStatement)node , writer );
                    break;
                case BoundNodeKind.ReturnStatement:
                    WriteReturnStatement( (BoundReturnStatement)node , writer );
                    break;
                case BoundNodeKind.ExpressionStatement:
                    WriteExpressionStatement( (BoundExpressionStatement)node , writer );
                    break;
                case BoundNodeKind.ErrorExpression:
                    WriteErrorExpression( (BoundErrorExpression)node , writer );
                    break;
                case BoundNodeKind.LiteralExpression:
                    WriteLiteralExpression( (BoundLiteralExpression)node , writer );
                    break;
                case BoundNodeKind.VariableExpression:
                    WriteVariableExpression( (BoundVariableExpression)node , writer );
                    break;
                case BoundNodeKind.AssignmentExpression:
                    WriteAssignmentExpression( (BoundAssignmentExpression)node , writer );
                    break;
                case BoundNodeKind.UnaryExpression:
                    WriteUnaryExpression( (BoundUnaryExpression)node , writer );
                    break;
                case BoundNodeKind.BinaryExpression:
                    WriteBinaryExpression( (BoundBinaryExpression)node , writer );
                    break;
                case BoundNodeKind.CallExpression:
                    WriteCallExpression( (BoundCallExpression)node , writer );
                    break;
                case BoundNodeKind.ConversionExpression:
                    WriteConversionExpression( (BoundConversionExpression)node , writer );
                    break;
                default:
                    throw new Exception( $"Unexpected node {node.Kind}" );
            }
        }

        private static void WriteNestedStatement( this IndentedTextWriter writer , BoundStatement node )
        {
            var needsIndentation = !(node is BoundBlockStatement);

            if( needsIndentation )
                writer.Indent++;

            node.WriteTo( writer );

            if( needsIndentation )
                writer.Indent--;
        }

        private static void WriteNestedExpression( this IndentedTextWriter writer , int parentPrecedence , BoundExpression expression )
        {
            if( expression is BoundUnaryExpression unary )
                writer.WriteNestedExpression( parentPrecedence , unary.Op.TkType.GetUnaryOperatorPrecedence() , unary );
            else if( expression is BoundBinaryExpression binary )
                writer.WriteNestedExpression( parentPrecedence , binary.Operator.TkType.GetBinaryOperatorPrecedence() , binary );
            else
                expression.WriteTo( writer );
        }

        private static void WriteNestedExpression( this IndentedTextWriter writer , int parentPrecedence , int currentPrecedence , BoundExpression expression )
        {
            var needsParenthesis = parentPrecedence >= currentPrecedence;

            if( needsParenthesis )
                writer.WritePunctuation( TokenType.OpenParen );

            expression.WriteTo( writer );

            if( needsParenthesis )
                writer.WritePunctuation( TokenType.CloseParen );
        }

        private static void WriteBlockStatement( BoundBlockStatement node , IndentedTextWriter writer )
        {
            writer.WritePunctuation( TokenType.OpenBrace );
            writer.WriteLine();
            writer.Indent++;

            foreach( var s in node.Statements )
                s.WriteTo( writer );

            writer.Indent--;
            writer.WritePunctuation( TokenType.CloseBrace );
            writer.WriteLine();
        }

        private static void WriteVariableDeclaration( BoundVariableDeclaration node , IndentedTextWriter writer )
        {
            writer.Write( node.Variable.IsReadOnly ? TokenType.Let : TokenType.Var );
            writer.WriteSpace();
            writer.WriteIdentifier( node.Variable.Name );
            writer.WriteSpace();
            writer.WritePunctuation( TokenType.Eq );
            writer.WriteSpace();
            node.Initializer.WriteTo( writer );
            writer.WriteLine();
        }

        private static void WriteIfStatement( BoundIfStatement node , IndentedTextWriter writer )
        {
            writer.Write( TokenType.If );
            writer.WriteSpace();
            node.Condition.WriteTo( writer );
            writer.WriteLine();
            writer.WriteNestedStatement( node.ThenStatement );

            if( node.ElseStatement != null )
            {
                writer.Write( TokenType.Else );
                writer.WriteLine();
                writer.WriteNestedStatement( node.ElseStatement );
            }
        }

        private static void WriteWhileStatement( BoundWhileStatement node , IndentedTextWriter writer )
        {
            writer.Write( TokenType.While );
            writer.WriteSpace();
            node.Condition.WriteTo( writer );
            writer.WriteLine();
            writer.WriteNestedStatement( node.Body );
        }

        private static void WriteDoWhileStatement( BoundDoWhileStatement node , IndentedTextWriter writer )
        {
            writer.Write( TokenType.Do );
            writer.WriteLine();
            writer.WriteNestedStatement( node.Body );
            writer.Write( TokenType.While );
            writer.WriteSpace();
            node.Condition.WriteTo( writer );
            writer.WriteLine();
        }

        private static void WriteForStatement( BoundForStatement node , IndentedTextWriter writer )
        {
            writer.Write( TokenType.For );
            writer.WriteSpace();
            writer.WriteIdentifier( node.Variable.Name );
            writer.WriteSpace();
            writer.WritePunctuation( TokenType.Eq );
            writer.WriteSpace();
            node.LowerBound.WriteTo( writer );
            writer.WriteSpace();
            writer.Write( TokenType.To );
            writer.WriteSpace();
            node.UpperBound.WriteTo( writer );
            writer.WriteLine();
            writer.WriteNestedStatement( node.Body );
        }

        private static void WriteLabelStatement( BoundLabelStatement node , IndentedTextWriter writer )
        {
            var unindent = writer.Indent > 0;
            if( unindent )
                writer.Indent--;

            writer.WritePunctuation( node.Label.Name );
            writer.WritePunctuation( TokenType.Colon );
            writer.WriteLine();

            if( unindent )
                writer.Indent++;
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

        private static void WriteReturnStatement( BoundReturnStatement node , IndentedTextWriter writer )
        {
            writer.Write( TokenType.Return );
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
                writer.Write( (bool)node.Value ? TokenType.True : TokenType.False );
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
            writer.WritePunctuation( TokenType.Eq );
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
            writer.WritePunctuation( TokenType.OpenParen );

            var isFirst = true;
            foreach( var argument in node.Arguments )
            {
                if( isFirst )
                {
                    isFirst = false;
                }
                else
                {
                    writer.WritePunctuation( TokenType.Comma );
                    writer.WriteSpace();
                }

                argument.WriteTo( writer );
            }

            writer.WritePunctuation( TokenType.CloseParen );
        }

        private static void WriteConversionExpression( BoundConversionExpression node , IndentedTextWriter writer )
        {
            writer.WriteIdentifier( node.Type.Name );
            writer.WritePunctuation( TokenType.OpenParen );
            node.Expression.WriteTo( writer );
            writer.WritePunctuation( TokenType.CloseParen );
        }
    }

}
