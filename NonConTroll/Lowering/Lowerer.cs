using System.Collections.Generic;
using System.Collections.Immutable;
using System;
using System.Diagnostics;
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

        private BoundLabelStatement GenerateLabelStatement()
        {
            var label = this.GenerateLabel();

            return new BoundLabelStatement( label );
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
                    {
                        stack.Push( s );
                    }
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
                var endLabel          = this.GenerateLabel();
                var gotoFalse         = new BoundConditionalGotoStatement( endLabel , node.Condition , false );
                var endLabelStatement = new BoundLabelStatement( endLabel );
                var result = new BoundBlockStatement( ImmutableArray.Create( gotoFalse , node.ThenStatement , endLabelStatement ) );

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

                var elseLabel          = this.GenerateLabel();
                var endLabel           = this.GenerateLabel();
                var gotoFalse          = new BoundConditionalGotoStatement( elseLabel , node.Condition , false );
                var gotoEndStatement   = new BoundGotoStatement( endLabel );
                var elseLabelStatement = new BoundLabelStatement( elseLabel );
                var endLabelStatement  = new BoundLabelStatement( endLabel );
                var result = new BoundBlockStatement( ImmutableArray.Create(
                    gotoFalse , node.ThenStatement , gotoEndStatement , elseLabelStatement , node.ElseStatement , endLabelStatement
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

            var bodyLabel              = this.GenerateLabel();
            var gotoContinue           = new BoundGotoStatement( node.ContinueLabel );
            var bodyLabelStatement     = new BoundLabelStatement( bodyLabel );
            var continueLabelStatement = new BoundLabelStatement( node.ContinueLabel );
            var gotoTrue               = new BoundConditionalGotoStatement( bodyLabel , node.Condition );
            var breakLabelStatement    = new BoundLabelStatement( node.BreakLabel );
            var result = new BoundBlockStatement( ImmutableArray.Create(
                gotoContinue , bodyLabelStatement , node.Body , continueLabelStatement , gotoTrue , breakLabelStatement
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
            var bodyLabelStatement     = new BoundLabelStatement( bodyLabel );
            var continueLabelStatement = new BoundLabelStatement( node.ContinueLabel );
            var gotoTrue               = new BoundConditionalGotoStatement( bodyLabel , node.Condition );
            var breakLabelStatement    = new BoundLabelStatement( node.BreakLabel );
            var result = new BoundBlockStatement( ImmutableArray.Create(
                bodyLabelStatement , node.Body , continueLabelStatement , gotoTrue , breakLabelStatement
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

            var variableDeclaration   = new BoundVariableDeclaration( node.Variable , node.LowerBound );
            var variableExpression    = new BoundVariableExpression( node.Variable );
            var upperBoundSymbol      = new LocalVariableSymbol( "upperBound" , true , TypeSymbol.Int );
            var upperBoundDeclaration = new BoundVariableDeclaration( upperBoundSymbol , node.UpperBound );
            var condition = new BoundBinaryExpression(
                variableExpression , BoundBinaryOperator.Bind( TokenType.LtEq , TypeSymbol.Int , TypeSymbol.Int )! ,
                new BoundVariableExpression( upperBoundSymbol )
            );
            var continueLabelStatement = new BoundLabelStatement( node.ContinueLabel );
            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression( node.Variable ,
                    new BoundBinaryExpression( variableExpression ,
                                               BoundBinaryOperator.Bind( TokenType.Plus , TypeSymbol.Int , TypeSymbol.Int )! ,
                                               new BoundLiteralExpression( 1 ) )
                )
            );
            var whileBody      = new BoundBlockStatement( ImmutableArray.Create( node.Body , continueLabelStatement , increment ) );
            var whileStatement = new BoundWhileStatement( condition , whileBody , node.BreakLabel , this.GenerateLabel() );
            var result = new BoundBlockStatement( ImmutableArray.Create<BoundStatement>(
                variableDeclaration , upperBoundDeclaration , whileStatement
            ) );

            return this.RewriteStatement( result );
        }

        protected override BoundExpression RewriteMatchExpression( BoundMatchExpression node )
        {
            return node;

            /*
                match <expr> {
                    <exprA> => <thenA> ,
                    <exprB> , <exprC> => <thenBC> ,
                    _ => <exprD> ,
                }

                ---->

                let <var> = <expr>

                if <var> == <exprA>
                    <thenA>
                elif <var> == <exprB> or <var> == <exprC>
                    <thenBC>
            */

            var statements = new List<BoundStatement>();

            var exprVariableSymbol = new LocalVariableSymbol( "matchExpression" , isReadOnly: true , node.Expression.Type );
            var exprVariableDecl   = new BoundVariableDeclaration( exprVariableSymbol , node.Expression );
            var exprVariableExpr   = new BoundVariableExpression( exprVariableSymbol );

            var endLabel     = this.GenerateLabel();
            var endLabelStmt = new BoundLabelStatement( endLabel );

            foreach( var patternSection in node.PatternSections )
            {
                var resultBlock = default( BoundStatement );

                if( patternSection.Result is BoundExpression expr )
                {
                    resultBlock = new BoundExpressionStatement( (BoundExpression)patternSection.Result );
                }
                else if( patternSection.Result is BoundStatement stmt )
                {
                    resultBlock = stmt;
                }
                else
                {
                    throw new Exception();
                }

                // if( node.IsStatement )
                // {
                //     resultBlock = (BoundStatement)patternSection.Result;
                // }
                // else
                // {
                //     Debug.Assert( patternSection.Result is BoundExpression );

                //     resultBlock = new BoundExpressionStatement( (BoundExpression)patternSection.Result );
                // }

                var resultLabelStmt = this.GenerateLabelStatement();

                if( patternSection.Patterns.Count() > 1 )
                {
                    foreach( var patternExpr in patternSection.Patterns )
                    {
                        // TODO: binary || via recursion
                    }

                    throw new NotImplementedException();
                }
                else
                {
                    var pattern = patternSection.Patterns.Single();
                    var patternExpr = this.ConvertPatternToExpression( exprVariableExpr , pattern );
                    var gotoFalse = new BoundConditionalGotoStatement( endLabel , patternExpr , false );
                    var stmt = new BoundBlockStatement( ImmutableArray.Create( gotoFalse , resultBlock , endLabelStmt ) );

                    statements.Add( stmt );
                }
            }

            statements.Add( endLabelStmt );

            var blockStmt = new BoundBlockStatement( statements.ToImmutableArray() );
            var resultStmt = this.RewriteStatement( blockStmt );

            if( node.IsStatement )
            {

            }

            return null;
        }

        private BoundIfStatement GenerateIfStatement( BoundVariableExpression variableExpr )
        {
            return null;
        }

        private BoundBinaryExpression GenerateBinaryExpression( BoundVariableExpression variableExpr )
        {
            return null;
        }

        private BoundExpression ConvertPatternToExpression( BoundExpression lhs , BoundPattern pattern )
        {
            switch( pattern.Kind )
            {
                case BoundNodeKind.MatchAnyPattern:
                {
                    var expr = new BoundLiteralExpression( true );

                    return expr;
                }

                case BoundNodeKind.ConstantPattern:
                {
                    var constantPattern = (BoundConstantPattern)pattern;
                    var comparison = BoundBinaryOperator.Bind( TokenType.Eq , lhs.Type , constantPattern.Expression.Type )!;
                    var expr = new BoundBinaryExpression( lhs , comparison , constantPattern.Expression );

                    return expr;
                }

                // TODO: when infix function parsing (binary and unary!) is done
                // case BoundNodeKind.InfixPattern:
                // {
                //     var infixPattern = (BoundConstantPattern)pattern;
                //     var comparison = BoundBinaryOperator.Bind( TokenType.Eq , lhs.Type , infixPattern.Expression.Type )!;

                //     var args = ImmutableArray.Create( lhs , infixPattern.Expression );
                //     var expr = new BoundCallExpression( infixFuncSymbol , args );

                //     return expr;
                // }

                default:
                {
                    throw new Exception( "unreachable" );
                }
            }
        }



    }
}
