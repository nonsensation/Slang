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
            var result = new BoundBlockStatement( new[] { bodyLabelStatement , node.Body , continueLabelStatement , gotoTrue , breakLabelStatement }.ToImmutableArray() );

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
            var variableExpression  = new BoundVariableExpression( node.Variable );

            var boundSymbol       = new LocalVariableSymbol( "upperBound" , true , TypeSymbol.Int );
            var boundDecl         = new BoundVariableDeclaration( boundSymbol , node.UpperBound );
            var boundExpr         = new BoundVariableExpression( boundSymbol );
            var boundComparisonOp = BoundBinaryOperator.Bind( SyntaxKind.LtEqToken , TypeSymbol.Int , TypeSymbol.Int );
            var condition         = new BoundBinaryExpression( variableExpression , boundComparisonOp! , boundExpr );

            var incrOp     = BoundBinaryOperator.Bind( SyntaxKind.PlusToken , TypeSymbol.Int , TypeSymbol.Int );
            var incrByOne  = new BoundBinaryExpression( variableExpression , incrOp! , new BoundLiteralExpression( 1 ) );
            var incrAssign = new BoundAssignmentExpression( node.Variable , incrByOne );
            var increment  = new BoundExpressionStatement( incrAssign );

            var continueLabelStatement = new BoundLabelStatement( node.ContinueLabel );

            var whileBody      = new BoundBlockStatement( ImmutableArray.Create( node.Body , continueLabelStatement , increment ) );
            var whileStatement = new BoundWhileStatement( condition , whileBody , node.BreakLabel , this.GenerateLabel() );

            var result = new BoundBlockStatement( ImmutableArray.Create<BoundStatement>( variableDeclaration , boundDecl , whileStatement ) );

            return this.RewriteStatement( result );
        }

        protected override BoundStatement RewriteMatchStatement( BoundMatchStatement node )
        {
            /*
                match <expr> {
                    <exprA> => <stmtA> ,
                    <exprB> , <exprC> => <stmtBC> ,
                    _ => <stmtMatchAny> ,
                }

                ---->

                let <var> = <expr>

                if <var> == <exprA>
                    <stmtA>
                elif <var> == <exprB> or <var> == <exprC>
                    <stmtBC>
                else
                    <stmtMatchAny>
            */

            var exprVariableSymbol = new LocalVariableSymbol( "matchExpression" , isReadOnly: true , node.Expression.Type );
            var exprVariableDecl   = new BoundVariableDeclaration( exprVariableSymbol , node.Expression );
            var exprVariableExpr   = new BoundVariableExpression( exprVariableSymbol );

            var prevStmt = default( BoundStatement );

            if( node.DefaultPattern != null )
            {
                if( node.PatternSections.Count() == 1 )
                {
                    // match has only 1 section containing the match-any case
                    // all other scenarios should be a copile error
                    var matchAnySection = node.PatternSections.Single();
                    var boundBlockStmt = new BoundBlockStatement( ImmutableArray.Create( exprVariableDecl , matchAnySection.Statement ) );

                    return this.RewriteStatement( boundBlockStmt );
                }

                prevStmt = this.RewriteStatement( node.DefaultPattern.Statement );
            }

            foreach( var patternSection in node.PatternSections.Reverse() )
            {
                if( node.DefaultPattern != null && node.DefaultPattern == patternSection )
                {
                    continue;
                }

                var prevCondition = default( BoundExpression );

                foreach( var pattern in patternSection.Patterns.Reverse() )
                {
                    var condition = this.ConvertPatternToExpression( pattern , exprVariableExpr );

                    if( prevCondition != null )
                    {
                        var binaryOp = BoundBinaryOperator.Bind( SyntaxKind.PipePipeToken , condition.Type , prevCondition.Type )!;

                        condition = new BoundBinaryExpression( condition , binaryOp , prevCondition );
                    }

                    prevCondition = condition;
                }

                var ifStmt = new BoundIfStatement( prevCondition! , patternSection.Statement , prevStmt );

                prevStmt = ifStmt;
            }

            var blockStmt = new BoundBlockStatement( ImmutableArray.Create( exprVariableDecl , prevStmt! ) );

            return this.RewriteStatement( blockStmt );
        }

        private BoundExpression ConvertPatternToExpression( BoundPattern pattern , BoundExpression matchExpr )
        {
            var expr = default( BoundExpression );

            switch( pattern )
            {
                case BoundMatchAnyPattern b:
                {
                    expr = new BoundLiteralExpression( true );
                }
                break;
                case BoundConstantPattern constantPattern:
                {
                    var comparison = BoundBinaryOperator.Bind( SyntaxKind.EqEqToken , matchExpr.Type , constantPattern.Expression.Type )!;

                    expr = new BoundBinaryExpression( matchExpr , comparison , constantPattern.Expression );
                }
                break;

                // TODO: when infix function parsing (binary and unary!) is done
                // case BoundInfixPattern infixPattern:
                // {
                //     var comparison = BoundBinaryOperator.Bind( TokenType.Eq , matchExpr.Type , infixPattern.Expression.Type )!;
                //
                //     var args = ImmutableArray.Create( matchExpr , infixPattern.Expression );
                //     var expr = new BoundCallExpression( infixFuncSymbol , args );
                //
                //     return expr;
                // }
                // break;

                default:
                {
                    throw new Exception( "unreachable" );
                }
            }

            return this.RewriteExpression( expr! );
        }

        private BoundStatement RewriteYieldStatement( YieldStatementSyntax node )
        {

        }



    }
}
