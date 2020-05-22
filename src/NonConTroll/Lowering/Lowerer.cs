using System.Collections.Generic;
using System.Collections.Immutable;
using System;
using System.Linq;
using NonConTroll.CodeAnalysis.Binding;
using NonConTroll.CodeAnalysis.Symbols;

using static NonConTroll.CodeAnalysis.Lowering.BoundNodeFactory;
using NonConTroll.CodeAnalysis.Syntax;
using System.Diagnostics;

namespace NonConTroll.CodeAnalysis.Lowering
{
    internal class Lowerer : BoundTreeRewriter
    {
        private int LabelCount;

        private Lowerer()
        {
        }

        public static BoundBlockStatement Lower( FunctionSymbol function , BoundStatement statement )
        {
            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement( statement );
            var flattenedResult = Flatten( function , result );
            var removedDeadCodeResult = RemoveDeadCode( flattenedResult );

            return removedDeadCodeResult;
        }

        private static BoundBlockStatement Flatten( FunctionSymbol function , BoundStatement statement )
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

            if( function.ReturnType == BuiltinTypes.Void )
            {
                if( builder.Count == 0 || CanFallThrough( builder.Last() ) )
                {
                    builder.Add( new BoundReturnStatement( null ) );
                }
            }

            return new BoundBlockStatement( builder.ToImmutable() );
        }

        private static bool CanFallThrough( BoundStatement boundStatement )
        {
            return boundStatement.Kind != BoundNodeKind.ReturnStatement &&
                   boundStatement.Kind != BoundNodeKind.GotoStatement;
        }

        private static BoundBlockStatement RemoveDeadCode( BoundBlockStatement node )
        {
            var controlFlow = ControlFlowGraph.Create( node );
            var reachableStatements = new HashSet<BoundStatement>(
                controlFlow.Blocks.SelectMany( b => b.Statements ) );
            var builder = node.Statements.ToBuilder();

            for( int i = builder.Count - 1 ; i >= 0 ; i-- )
            {
                if( !reachableStatements.Contains( builder[ i ] ) )
                {
                    builder.RemoveAt( i );
                }
            }

            return new BoundBlockStatement( builder.ToImmutable() );
        }

        #region Rewrite

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

                var result = If( node.Condition , node.ThenStatement );

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

                var result = If( node.Condition , node.ThenStatement , node.ElseStatement );

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

            var bodyLabel = Label();
            var gotoContinue = Goto( node.ContinueLabel );
            var continueLabel = Label( node.ContinueLabel );
            var gotoTrue = GotoIf( bodyLabel , node.Condition );
            var breakLabel = Label( node.BreakLabel );
            var result = Block( gotoContinue , bodyLabel , node.Body , continueLabel , gotoTrue , breakLabel );

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

            var bodyLabel = Label();
            var continueLabel = Label( node.ContinueLabel );
            var gotoTrue = GotoIf( bodyLabel , node.Condition );
            var breakLabel = Label( node.BreakLabel );
            var result = Block( bodyLabel , node.Body , continueLabel , gotoTrue , breakLabel );

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
            //      break:
            // }

#if false // old, maybe use again
            var variableDecl = VarDecl( node.Variable , node.LowerBound );
            var variableExpr = Var( variableDecl );
            var boundSymbol = Symbol( "upperBound" , BuiltinTypes.Int , isReadOnly: true , node.UpperBound.ConstantValue );
            var boundDecl = VarDecl( boundSymbol , node.UpperBound );
            var boundExpr = Var( boundSymbol );
            var condition = LtEq( variableExpr , boundExpr );
            var increment = Incr( variableExpr );
            var continueLabel = Label( node.ContinueLabel );
            var whileBody = Block( node.Body , continueLabel , increment );
            var whileStatement = While( condition , whileBody , node.BreakLabel );
            var result = Block( variableDecl , boundDecl , whileStatement );
#else // this creates Var( variableDecl ) multiple times
            var lowerBound = VariableDeclaration( node.Variable , node.LowerBound );
            var upperBound = Symbol( "upperBound" , BuiltinTypes.Int , isReadOnly: true , node.UpperBound.ConstantValue );
            var result = Block( lowerBound ,
                                VariableDeclaration( upperBound , node.UpperBound ) ,
                                While( condition: CompareLessThanOrEqual( Variable( lowerBound ) , Variable( upperBound ) ) ,
                                            body: Block( node.Body , Label( node.ContinueLabel ) , Increment( Variable( lowerBound ) ) ) ,
                                      breakLabel: node.BreakLabel ) );
#endif
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

            var exprVariableSymbol = Symbol( "matchExpression" , node.Expression.Type , isReadOnly: true , node.Expression.ConstantValue );
            var exprVariableDecl = VariableDeclaration( exprVariableSymbol , node.Expression );
            var exprVariableExpr = Variable( exprVariableSymbol );

            var prevStmt = default( BoundStatement );

            if( node.DefaultPattern != null )
            {
                if( node.PatternSections.Count() == 1 )
                {
                    // match has only 1 section containing the match-any case
                    // all other scenarios should be a copile error
                    var matchAnySection = node.PatternSections.Single();
                    var boundBlockStmt = Block( exprVariableDecl , matchAnySection.Statement );

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
                        condition = Or( condition , prevCondition );
                    }

                    prevCondition = condition;
                }

                var ifStmt = If( prevCondition! , patternSection.Statement , prevStmt );

                prevStmt = ifStmt;
            }

            var blockStmt = Block( exprVariableDecl , prevStmt! );

            return this.RewriteStatement( blockStmt );
        }

        private BoundExpression ConvertPatternToExpression( BoundPattern pattern , BoundExpression matchExpr )
        {
            var expr = default( BoundExpression );

            switch( pattern )
            {
                case BoundMatchAnyPattern b:
                {
                    expr = Literal( true );
                }
                break;
                case BoundConstantPattern constantPattern:
                {
                    expr = CompareEqual( matchExpr , constantPattern.Expression );
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

        protected override BoundStatement RewriteConditionalGotoStatement( BoundConditionalGotoStatement node )
        {
            if( node.Condition.ConstantValue != null )
            {
                var value = (bool)node.Condition.ConstantValue.Value;
                var condition = node.JumpIfTrue ? value : !value;

                if( condition )
                {
                    return Goto( node.Label );
                }
                else
                {
                    //return new BoundNop();
                }
            }

            return base.RewriteConditionalGotoStatement( node );
        }

        #endregion

        #region Factory methods

        private BoundBlockStatement Block( params BoundStatement[] stmts )
        {
            return new BoundBlockStatement( ImmutableArray.Create( stmts ) );
        }

        private BoundUnaryExpression Not( BoundExpression expr )
            => UnExpr( SyntaxKind.ExmToken , expr );
        private BoundUnaryExpression Neg( BoundExpression expr )
            => UnExpr( SyntaxKind.MinusToken , expr );

        private BoundBinaryExpression CompareEqual( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.EqEqToken , rhs );
        private BoundBinaryExpression CompareNotEqual( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.ExmEqToken , rhs );
        private BoundBinaryExpression Add( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.PlusToken , rhs );
        private BoundBinaryExpression Sub( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.MinusToken , rhs );
        private BoundBinaryExpression Mul( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.StarToken , rhs );
        private BoundBinaryExpression Div( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.SlashToken , rhs );
        private BoundBinaryExpression CompareGreaterThan( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.GtToken , rhs );
        private BoundBinaryExpression CompareGreatherThanOrEqual( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.GtEqToken , rhs );
        private BoundBinaryExpression CompareLessThan( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.LtToken , rhs );
        private BoundBinaryExpression CompareLessThanOrEqual( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.LtEqToken , rhs );
        private BoundBinaryExpression And( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.AndAndToken , rhs );
        private BoundBinaryExpression Or( BoundExpression lhs , BoundExpression rhs )
            => BinExpr( lhs , SyntaxKind.PipePipeToken , rhs );

        private BoundBinaryExpression BinExpr( BoundExpression lhs , SyntaxKind kind , BoundExpression rhs )
        {
            var op = BoundBinaryOperator.Bind( SyntaxKind.EqEqToken , lhs.Type , rhs.Type )!;

            return new BoundBinaryExpression( lhs , op , rhs );
        }

        private BoundUnaryExpression UnExpr( SyntaxKind kind , BoundExpression expr )
        {
            var op = BoundUnaryOperator.Bind( SyntaxKind.EqEqToken , expr.Type )!;

            return new BoundUnaryExpression( op , expr );
        }

        private BoundLiteralExpression Literal( object literal )
        {
            Debug.Assert( literal is string || literal is bool || literal is int );

            return new BoundLiteralExpression( literal );
        }

        private BoundStatement If( BoundExpression condition , BoundStatement thenStmt )
        {
            var endLabel = Label();
            var gotoFalse = GotoIfFalse( endLabel , condition );

            return Block( gotoFalse , thenStmt , endLabel );
        }

        private BoundStatement If( BoundExpression condition , BoundStatement thenStmt , BoundStatement? elseStmt )
        {
            if( elseStmt == null )
            {
                return If( condition , thenStmt );
            }

            var elseLabel = Label();
            var endLabel = Label();
            var gotoFalse = GotoIfFalse( elseLabel , condition );
            var gotoEndStmt = Goto( endLabel );

            return Block( gotoFalse , thenStmt , gotoEndStmt , elseLabel , elseStmt , endLabel );
        }

        private BoundGotoStatement Goto( BoundLabelStatement label )
        {
            return new BoundGotoStatement( label.Label );
        }

        private BoundGotoStatement Goto( BoundLabel label )
        {
            return new BoundGotoStatement( label );
        }

        private BoundConditionalGotoStatement GotoIf( BoundLabelStatement label , BoundExpression condition , bool jumpIfTrue = true )
            => new BoundConditionalGotoStatement( label.Label , condition , jumpIfTrue );

        private BoundConditionalGotoStatement GotoIfTrue( BoundLabelStatement label , BoundExpression condition )
            => new BoundConditionalGotoStatement( label.Label , condition , true );

        private BoundConditionalGotoStatement GotoIfFalse( BoundLabelStatement label , BoundExpression condition )
            => new BoundConditionalGotoStatement( label.Label , condition , false );


        private BoundLabel GenerateLabel()
        {
            var name = $"Label{++this.LabelCount}";

            return new BoundLabel( name );
        }

        private BoundLabelStatement Label()
        {
            var label = this.GenerateLabel();

            return new BoundLabelStatement( label );
        }

        private BoundLabelStatement Label( BoundLabel label )
        {
            return new BoundLabelStatement( label );
        }

        private BoundVariableDeclaration VariableDeclaration( VariableSymbol symbol , BoundExpression initExpr )
        {
            return new BoundVariableDeclaration( symbol , initExpr );
        }

        private BoundVariableExpression Variable( VariableSymbol symbol )
        {
            return new BoundVariableExpression( symbol );
        }

        private BoundVariableExpression Variable( BoundVariableDeclaration varDecl )
        {
            return new BoundVariableExpression( varDecl.Variable );
        }

        private LocalVariableSymbol Symbol( string name , TypeSymbol type , bool isReadOnly = true , BoundConstant? constant = null )
        {
            return new LocalVariableSymbol( name , isReadOnly , type , constant );
        }

        private BoundWhileStatement While( BoundExpression condition , BoundStatement body , BoundLabel breakLabel )
        {
            var continueLabel = this.GenerateLabel();

            return new BoundWhileStatement( condition , body , breakLabel , continueLabel );
        }

        private BoundExpressionStatement Increment( BoundVariableExpression varExpr )
        {
            var incrByOne = Add( varExpr , Literal( 1 ) );
            var incrAssign = new BoundAssignmentExpression( varExpr.Variable , incrByOne );

            return new BoundExpressionStatement( incrAssign );
        }

        private BoundExpressionStatement Decrement( BoundVariableExpression varExpr )
        {
            var incrByOne = Sub( varExpr , Literal( 1 ) );
            var incrAssign = new BoundAssignmentExpression( varExpr.Variable , incrByOne );

            return new BoundExpressionStatement( incrAssign );
        }

        #endregion
    }
}
