using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;
using NonConTroll.CodeAnalysis.Syntax;

namespace NonConTroll.CodeAnalysis.Lowering
{
    internal class Lowerer : BoundTreeRewriter
    {
        private int LabelCount;

        private Lowerer()
        {
        }

        private BoundLabel GenerateLabel()
        {
            var name = $"Label{++this.LabelCount}";

            return new BoundLabel( name );
        }

        public static BoundBlockStatement Lower( BoundStatement statement )
        {
            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement( statement );

            return Flatten( result );
        }

        private static BoundBlockStatement Flatten( BoundStatement statement )
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();

            stack.Push( statement );

            while( stack.Count > 0 )
            {
                var current = stack.Pop();

                if( current is BoundBlockStatement block )
                {
                    foreach( var s in block.Statements.Reverse() )
                        stack.Push( s );
                }
                else
                {
                    builder.Add( current );
                }
            }

            return new BoundBlockStatement( builder.ToImmutable() );
        }

        protected override BoundStatement RewriteIfStatement( BoundIfStatement node )
        {
            if( node.ElseStatement == null )
            {
                // if <condition>
                //      <then>
                //
                // ---->
                //
                // gotoFalse <condition> end
                // <then>
                // end:
                var endLabel = this.GenerateLabel();
                var gotoFalse = new BoundConditionalGotoStatement( endLabel , node.Condition , false );
                var endLabelStatement = new BoundLabelStatement( endLabel );
                var result = new BoundBlockStatement( ImmutableArray.Create<BoundStatement>( gotoFalse , node.ThenStatement , endLabelStatement ) );

                return this.RewriteStatement( result );
            }
            else
            {
                // if <condition>
                //      <then>
                // else
                //      <else>
                //
                // ---->
                //
                // gotoFalse <condition> else
                // <then>
                // goto end
                // else:
                // <else>
                // end:

                var elseLabel = this.GenerateLabel();
                var endLabel = this.GenerateLabel();
                var gotoFalse = new BoundConditionalGotoStatement( elseLabel , node.Condition , false );
                var gotoEndStatement = new BoundGotoStatement( endLabel );
                var elseLabelStatement = new BoundLabelStatement( elseLabel );
                var endLabelStatement = new BoundLabelStatement( endLabel );
                var result = new BoundBlockStatement( ImmutableArray.Create<BoundStatement>(
                    gotoFalse ,
                    node.ThenStatement ,
                    gotoEndStatement ,
                    elseLabelStatement ,
                    node.ElseStatement ,
                    endLabelStatement
                ) );

                return this.RewriteStatement( result );
            }
        }

        protected override BoundStatement RewriteWhileStatement( BoundWhileStatement node )
        {
            // while <condition>
            //      <body>
            //
            // ----->
            //
            // goto continue
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            var bodyLabel = this.GenerateLabel();
            var gotoContinue = new BoundGotoStatement( node.ContinueLabel );
            var bodyLabelStatement = new BoundLabelStatement( bodyLabel );
            var continueLabelStatement = new BoundLabelStatement( node.ContinueLabel );
            var gotoTrue = new BoundConditionalGotoStatement( bodyLabel , node.Condition );
            var breakLabelStatement = new BoundLabelStatement( node.BreakLabel );
            var result = new BoundBlockStatement( ImmutableArray.Create<BoundStatement>(
                gotoContinue ,
                bodyLabelStatement ,
                node.Body ,
                continueLabelStatement ,
                gotoTrue ,
                breakLabelStatement
            ) );

            return this.RewriteStatement( result );
        }

        protected override BoundStatement RewriteDoWhileStatement( BoundDoWhileStatement node )
        {
            // do
            //      <body>
            // while <condition>
            //
            // ----->
            //
            // body:
            // <body>
            // continue:
            // gotoTrue <condition> body
            // break:

            var bodyLabel = this.GenerateLabel();
            var bodyLabelStatement = new BoundLabelStatement( bodyLabel );
            var continueLabelStatement = new BoundLabelStatement( node.ContinueLabel );
            var gotoTrue = new BoundConditionalGotoStatement( bodyLabel , node.Condition );
            var breakLabelStatement = new BoundLabelStatement( node.BreakLabel );
            var result = new BoundBlockStatement( ImmutableArray.Create(
                bodyLabelStatement ,
                node.Body ,
                continueLabelStatement ,
                gotoTrue ,
                breakLabelStatement
            ) );

            return this.RewriteStatement( result );
        }

        protected override BoundStatement RewriteForStatement( BoundForStatement node )
        {
            // for <var> = <lower> to <upper>
            //      <body>
            //
            // ---->
            //
            // {
            //      var <var> = <lower>
            //      let upperBound = <upper>
            //      while( <var> <= upperBound )
            //      {
            //          <body>
            //          continue:
            //          <var> = <var> + 1
            //      }
            // }

            var variableDeclaration = new BoundVariableDeclaration( node.Variable , node.LowerBound );
            var variableExpression = new BoundVariableExpression( node.Variable );
            var upperBoundSymbol = new LocalVariableSymbol( "upperBound" , true , TypeSymbol.Int );
            var upperBoundDeclaration = new BoundVariableDeclaration( upperBoundSymbol , node.UpperBound );
            var condition = new BoundBinaryExpression(
                variableExpression ,
                BoundBinaryOperator.Bind( TokenType.LtEq , TypeSymbol.Int , TypeSymbol.Int )! ,
                new BoundVariableExpression( upperBoundSymbol )
            );
            var continueLabelStatement = new BoundLabelStatement( node.ContinueLabel );
            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable ,
                    new BoundBinaryExpression(
                        variableExpression ,
                        BoundBinaryOperator.Bind( TokenType.Plus , TypeSymbol.Int , TypeSymbol.Int )! ,
                        new BoundLiteralExpression( 1 )
                    )
                )
            );
            var whileBody = new BoundBlockStatement( ImmutableArray.Create<BoundStatement>(
                    node.Body ,
                    continueLabelStatement ,
                    increment )
            );
            var whileStatement = new BoundWhileStatement( condition , whileBody , node.BreakLabel , this.GenerateLabel() );
            var result = new BoundBlockStatement( ImmutableArray.Create<BoundStatement>(
                variableDeclaration ,
                upperBoundDeclaration ,
                whileStatement
            ) );

            return this.RewriteStatement( result );
        }
    }
}
